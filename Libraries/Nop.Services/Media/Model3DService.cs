using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImageResizer;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Seo;

namespace Nop.Services.Media
{
    /// <summary>
    /// Model3D service
    /// </summary>
    public partial class Model3DService : IModel3DService
    {
        #region Const

        private const int MULTIPLE_THUMB_DIRECTORIES_LENGTH = 3;

        #endregion

        #region Fields

        private static readonly object s_lock = new object();

        private readonly IRepository<Model3D> _model3DRepository;
        private readonly IRepository<ProductModel3D> _productModel3DRepository;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pictureRepository">Picture repository</param>
        /// <param name="productPictureRepository">Product picture repository</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="logger">Logger</param>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="mediaSettings">Media settings</param>
        public Model3DService(IRepository<Model3D> model3DRepository,
            IRepository<ProductModel3D> productModel3DRepository,
            ISettingService settingService, IWebHelper webHelper,
            ILogger logger, IEventPublisher eventPublisher,
            MediaSettings mediaSettings)
        {
            this._model3DRepository = model3DRepository;
            this._productModel3DRepository = productModel3DRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._logger = logger;
            this._eventPublisher = eventPublisher;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculates picture dimensions whilst maintaining aspect
        /// </summary>
        /// <param name="originalSize">The original picture size</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="resizeType">Resize type</param>
        /// <param name="ensureSizePositive">A value indicatingh whether we should ensure that size values are positive</param>
        /// <returns></returns>
        protected virtual Size CalculateDimensions(Size originalSize, int targetSize, 
            ResizeType resizeType = ResizeType.LongestSide, bool ensureSizePositive = true)
        {
            var newSize = new Size();
            switch (resizeType)
            {
                case ResizeType.LongestSide:
                    if (originalSize.Height > originalSize.Width)
                    {
                        // portrait 
                        newSize.Width = (int)(originalSize.Width * (float)(targetSize / (float)originalSize.Height));
                        newSize.Height = targetSize;
                    }
                    else 
                    {
                        // landscape or square
                        newSize.Height = (int)(originalSize.Height * (float)(targetSize / (float)originalSize.Width));
                        newSize.Width = targetSize;
                    }
                    break;
                case ResizeType.Width:
                    newSize.Height = (int)(originalSize.Height * (float)(targetSize / (float)originalSize.Width));
                    newSize.Width = targetSize;
                    break;
                case ResizeType.Height:
                    newSize.Width = (int)(originalSize.Width * (float)(targetSize / (float)originalSize.Height));
                    newSize.Height = targetSize;
                    break;
                default:
                    throw new Exception("Not supported ResizeType");
            }

            if (ensureSizePositive)
            {
                if (newSize.Width < 1)
                    newSize.Width = 1;
                if (newSize.Height < 1)
                    newSize.Height = 1;
            }

            return newSize;
        }
        
        /// <summary>
        /// Returns the file extension from mime type.
        /// </summary>
        /// <param name="mimeType">Mime type</param>
        /// <returns>File extension</returns>
        protected virtual string GetFileExtensionFromMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;

            //also see System.Web.MimeMapping for more mime types

            string[] parts = mimeType.Split('/');
            string lastPart = parts[parts.Length - 1];
            switch (lastPart)
            {
                case "pjpeg":
                    lastPart = "jpg";
                    break;
                case "x-png":
                    lastPart = "png";
                    break;
                case "x-icon":
                    lastPart = "ico";
                    break;
            }
            return lastPart;
        }

        /// <summary>
        /// Loads a picture from file
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary</returns>
        protected virtual byte[] LoadModel3DFromFile(int model3DId, string mimeType)
        {
            string lastPart = GetFileExtensionFromMimeType(mimeType);
            string fileName = string.Format("{0}_0.{1}", model3DId.ToString("0000000"), lastPart);
            var filePath = GetModel3DLocalPath(fileName);
            if (!File.Exists(filePath))
                return new byte[0];
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Save picture on file system
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        protected virtual void SaveModel3DInFile(int modelId, byte[] model3DBinary, string mimeType)
        {
            string lastPart = GetFileExtensionFromMimeType(mimeType);
            string fileName = string.Format("{0}_0.{1}", modelId.ToString("0000000"), lastPart);
            File.WriteAllBytes(GetModel3DLocalPath(fileName), model3DBinary);
        }

        /// <summary>
        /// Delete a picture on file system
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual void DeleteModel3DOnFileSystem(Model3D model3D)
        {
            if (model3D == null)
                throw new ArgumentNullException("model3D");

            string lastPart = GetFileExtensionFromMimeType(model3D.MimeType);
            string fileName = string.Format("{0}_0.{1}", model3D.Id.ToString("0000000"), lastPart);
            string filePath = GetModel3DLocalPath(fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual void DeleteModel3DThumbs(Model3D model3D)
        {
            string filter = string.Format("{0}*.*", model3D.Id.ToString("0000000"));
            var thumbDirectoryPath = _webHelper.MapPath("~/content/images/thumbs");
            string[] currentFiles = System.IO.Directory.GetFiles(thumbDirectoryPath, filter, SearchOption.AllDirectories);
            foreach (string currentFileName in currentFiles)
            {
                var thumbFilePath = GetThumbLocalPath(currentFileName);
                File.Delete(thumbFilePath);
            }
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected virtual string GetThumbLocalPath(string thumbFileName)
        {
            var thumbsDirectoryPath = _webHelper.MapPath("~/content/images/thumbs");
            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > MULTIPLE_THUMB_DIRECTORIES_LENGTH)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, MULTIPLE_THUMB_DIRECTORIES_LENGTH);
                    thumbsDirectoryPath = Path.Combine(thumbsDirectoryPath, subDirectoryName);
                    if (!System.IO.Directory.Exists(thumbsDirectoryPath))
                    {
                        System.IO.Directory.CreateDirectory(thumbsDirectoryPath);
                    }
                }
            }
            var thumbFilePath = Path.Combine(thumbsDirectoryPath, thumbFileName);
            return thumbFilePath;
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected virtual string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            storeLocation = !String.IsNullOrEmpty(storeLocation)
                                    ? storeLocation
                                    : _webHelper.GetStoreLocation();
            var url = storeLocation + "content/images/thumbs/";

            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > MULTIPLE_THUMB_DIRECTORIES_LENGTH)
                {
                    var subDirectoryName = fileNameWithoutExtension.Substring(0, MULTIPLE_THUMB_DIRECTORIES_LENGTH);
                    url = url + subDirectoryName + "/";
                }
            }

            url = url + thumbFileName;
            return url;
        }

        /// <summary>
        /// Get picture local path. Used when images stored on file system (not in the database)
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="imagesDirectoryPath">Directory path with images; if null, then default one is used</param>
        /// <returns>Local picture path</returns>
        protected virtual string GetModel3DLocalPath(string fileName, string imagesDirectoryPath = null)
        {
            if (String.IsNullOrEmpty(imagesDirectoryPath))
            {
                imagesDirectoryPath = _webHelper.MapPath("~/content/images/");
            }
            var filePath = Path.Combine(imagesDirectoryPath, fileName);
            return filePath;
        }

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <param name="fromDb">Load from database; otherwise, from file system</param>
        /// <returns>Picture binary</returns>
        protected virtual byte[] LoadModel3DBinary(Model3D model3D, bool fromDb)
        {
            if (model3D == null)
                throw new ArgumentNullException("model3D");

            var result = fromDb
                ? model3D.Model3DBinary
                : LoadModel3DFromFile(model3D.Id, model3D.MimeType);
            return result;
        }

        #endregion

        #region Getting model3D local path/URL methods

        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <returns>Picture binary</returns>
        public virtual byte[] LoadModel3DBinary(Model3D model3D)
        {
            return LoadModel3DBinary(model3D, this.StoreInDb);
        }

        /// <summary>
        /// Get picture SEO friendly name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Result</returns>
        public virtual string GetModel3DSeName(string name)
        {
            return SeoExtensions.GetSeName(name, true, false);
        }

        /// <summary>
        /// Gets the default picture URL
        /// </summary>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Picture URL</returns>
        public virtual string GetDefaultModel3DUrl(int targetSize = 0, 
            PictureType defaultPictureType = PictureType.Entity,
            string storeLocation = null)
        {
            string defaultImageFileName;
            switch (defaultPictureType)
            {
                case PictureType.Entity:
                    defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.gif");
                    break;
                case PictureType.Avatar:
                    defaultImageFileName = _settingService.GetSettingByKey("Media.Customer.DefaultAvatarImageName", "default-avatar.jpg");
                    break;
                default:
                    defaultImageFileName = _settingService.GetSettingByKey("Media.DefaultImageName", "default-image.gif");
                    break;
            }

            string filePath = GetModel3DLocalPath(defaultImageFileName,
                imagesDirectoryPath: _settingService.GetSettingByKey<string>("Media.DefaultImageDirectoryPath"));

            if (!File.Exists(filePath))
            {
                return "";
            }
            if (targetSize == 0)
            {
                string url = (!String.IsNullOrEmpty(storeLocation)
                                 ? storeLocation
                                 : _webHelper.GetStoreLocation())
                                 + "content/images/" + defaultImageFileName;
                return url;
            }
            else
            {
                string fileExtension = Path.GetExtension(filePath);
                string thumbFileName = string.Format("{0}_{1}{2}",
                    Path.GetFileNameWithoutExtension(filePath),
                    targetSize,
                    fileExtension);
                var thumbFilePath = GetThumbLocalPath(thumbFileName);
                if (!File.Exists(thumbFilePath))
                {
                    using (var b = new Bitmap(filePath))
                    {
                        var newSize = CalculateDimensions(b.Size, targetSize);

                        var destStream = new MemoryStream();
                        ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                        {
                            Width = newSize.Width,
                            Height = newSize.Height,
                            Scale = ScaleMode.Both,
                            Quality = _mediaSettings.DefaultImageQuality
                        });
                        var destBinary = destStream.ToArray();
                        File.WriteAllBytes(thumbFilePath, destBinary);
                    }
                }
                var url = GetThumbUrl(thumbFileName, storeLocation);
                return url;
            }
        }

        /// <summary>
        /// Get a picture URL
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <returns>Picture URL</returns>
        public virtual string GetModel3DUrl(int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true, 
            string storeLocation = null, 
            PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = GetModel3DById(pictureId);
            return GetModel3DUrl(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType);
        }
        
        /// <summary>
        /// Get a picture URL
        /// </summary>
        /// <param name="picture">Picture instance</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <returns>Picture URL</returns>
        public virtual string GetModel3DUrl(Model3D model3D, 
            int targetSize = 0,
            bool showDefaultPicture = true, 
            string storeLocation = null, 
            PictureType defaultPictureType = PictureType.Entity)
        {
            string url = string.Empty;
            byte[] model3DBinary = null;
            if (model3D != null)
                model3DBinary = LoadModel3DBinary(model3D);
            if (model3D == null || model3DBinary == null || model3DBinary.Length == 0)
            {
                if(showDefaultPicture)
                {
                    url = GetDefaultModel3DUrl(targetSize, defaultPictureType, storeLocation);
                }
                return url;
            }

            string lastPart = GetFileExtensionFromMimeType(model3D.MimeType);
            string thumbFileName;
            if (model3D.IsNew)
            {
                DeleteModel3DThumbs(model3D);

                //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                model3D = UpdateModel3D(model3D.Id, 
                    model3DBinary, 
                    model3D.MimeType, 
                    model3D.SeoFilename, 
                    false, 
                    false);
            }
            lock (s_lock)
            {
                string seoFileName = model3D.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure
                if (targetSize == 0)
                {
                    thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                        string.Format("{0}_{1}.{2}", model3D.Id.ToString("0000000"), seoFileName, lastPart) :
                        string.Format("{0}.{1}", model3D.Id.ToString("0000000"), lastPart);
                    var thumbFilePath = GetThumbLocalPath(thumbFileName);
                    if (!File.Exists(thumbFilePath))
                    {
                        File.WriteAllBytes(thumbFilePath, model3DBinary);
                    }
                }
                else
                {
                    thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
                        string.Format("{0}_{1}_{2}.{3}", model3D.Id.ToString("0000000"), seoFileName, targetSize, lastPart) :
                        string.Format("{0}_{1}.{2}", model3D.Id.ToString("0000000"), targetSize, lastPart);
                    var thumbFilePath = GetThumbLocalPath(thumbFileName);
                    if (!File.Exists(thumbFilePath))
                    {
                        using (var stream = new MemoryStream(model3DBinary))
                        {
                            Bitmap b = null;
                            try
                            {
                                //try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
                                b = new Bitmap(stream);
                            }
                            catch (ArgumentException exc)
                            {
                                _logger.Error(string.Format("Error generating picture thumb. ID={0}", model3D.Id), exc);
                            }
                            if (b == null)
                            {
                                //bitmap could not be loaded for some reasons
                                return url;
                            }

                            var newSize = CalculateDimensions(b.Size, targetSize);

                            var destStream = new MemoryStream();
                            ImageBuilder.Current.Build(b, destStream, new ResizeSettings
                            {
                                Width = newSize.Width,
                                Height = newSize.Height,
                                Scale = ScaleMode.Both,
                                Quality = _mediaSettings.DefaultImageQuality
                            });
                            var destBinary = destStream.ToArray();
                            File.WriteAllBytes(thumbFilePath, destBinary);

                            b.Dispose();
                        }
                    }
                }
            }
            url = GetThumbUrl(thumbFileName, storeLocation);
            return url;
        }

        /// <summary>
        /// Get a picture local path
        /// </summary>
        /// <param name="picture">Picture instance</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <returns></returns>
        public virtual string GetThumbLocalPath(Model3D model3D, int targetSize = 0, bool showDefaultPicture = true)
        {
            string url = GetModel3DUrl(model3D, targetSize, showDefaultPicture);
            if(String.IsNullOrEmpty(url))
                return String.Empty;
            
            return GetThumbLocalPath(Path.GetFileName(url));
        }

        #endregion

        #region CRUD methods

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        public virtual Model3D GetModel3DById(int model3DId)
        {
            if (model3DId == 0)
                return null;

            return _model3DRepository.GetById(model3DId);
        }

        /// <summary>
        /// Deletes a picture
        /// </summary>
        /// <param name="picture">Picture</param>
        public virtual void DeleteModel3D(Model3D model3D)
        {
            if (model3D == null)
                throw new ArgumentNullException("model3D");

            //delete thumbs
            // DOROBIT: DeletePictureThumbs(picture);
            
            //delete from file system
            if (!this.StoreInDb)
                DeleteModel3DOnFileSystem(model3D);

            //delete from database
            _model3DRepository.Delete(model3D);

            //event notification
            _eventPublisher.EntityDeleted(model3D);
        }

        /// <summary>
        /// Gets a collection of pictures
        /// </summary>
        /// <param name="pageIndex">Current page</param>
        /// <param name="pageSize">Items on each page</param>
        /// <returns>Paged list of pictures</returns>
        public virtual IPagedList<Model3D> GetModel3Ds(int pageIndex, int pageSize)
        {
            var query = from p in _model3DRepository.Table
                       orderby p.Id descending
                       select p;
            var pics = new PagedList<Model3D>(query, pageIndex, pageSize);
            return pics;
        }
        

        /// <summary>
        /// Gets pictures by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <returns>Pictures</returns>
        public virtual IList<Model3D> GetModel3DsByProductId(int productId, int recordsToReturn = 0)
        {
            if (productId == 0)
                return new List<Model3D>();

            
            var query = from p in _model3DRepository.Table
                        join pp in _productModel3DRepository.Table on p.Id equals pp.Model3DId
                        orderby pp.DisplayOrder
                        where pp.ProductId == productId
                        select p;

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = query.ToList();
            return pics;
        }

        /// <summary>
        /// Inserts a picture
        /// </summary>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        /// <returns>Picture</returns>
        public virtual Model3D InsertModel3D(byte[] model3DBinary, string mimeType, string seoFilename,
            bool isNew, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                model3DBinary = ValidateModel3D(model3DBinary, mimeType);

            var model3D = new Model3D
                              {
                                  Model3DBinary = this.StoreInDb ? model3DBinary : new byte[0],
                                  MimeType = mimeType,
                                  SeoFilename = seoFilename,
                                  IsNew = isNew,
                              };
            _model3DRepository.Insert(model3D);

            if(!this.StoreInDb)
                SaveModel3DInFile(model3D.Id, model3DBinary, mimeType);
            
            //event notification
            _eventPublisher.EntityInserted(model3D);

            return model3D;
        }

        /// <summary>
        /// Updates the picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        /// <returns>Picture</returns>
        public virtual Model3D UpdateModel3D(int model3DId, byte[] model3DBinary, string mimeType,
            string seoFilename, bool isNew, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                model3DBinary = ValidateModel3D(model3DBinary, mimeType);

            var model3D = GetModel3DById(model3DId);
            if (model3D == null)
                return null;

            //delete old thumbs if a picture has been changed
            if (seoFilename != model3D.SeoFilename)
                DeleteModel3DThumbs(model3D);

            model3D.Model3DBinary = (this.StoreInDb ? model3DBinary : new byte[0]);
            model3D.MimeType = mimeType;
            model3D.SeoFilename = seoFilename;
            model3D.IsNew = isNew;

            _model3DRepository.Update(model3D);

            if(!this.StoreInDb)
                SaveModel3DInFile(model3D.Id, model3DBinary, mimeType);

            //event notification
            _eventPublisher.EntityUpdated(model3D);

            return model3D;
        }

        /// <summary>
        /// Updates a SEO filename of a picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <returns>Picture</returns>
        public virtual Model3D SetSeoFilename(int model3DId, string seoFilename)
        {
            var model3D = GetModel3DById(model3DId);
            if (model3D == null)
                throw new ArgumentException("No model3D found with the specified id");

            //update if it has been changed
            if (seoFilename != model3D.SeoFilename)
            {
                //update picture
                model3D = UpdateModel3D(model3D.Id, LoadModel3DBinary(model3D), model3D.MimeType, seoFilename, true, false);
            }
            return model3D;
        }

        /// <summary>
        /// Validates input picture dimensions
        /// </summary>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary or throws an exception</returns>
        public virtual byte[] ValidateModel3D(byte[] model3DBinary, string mimeType)
        {
            var destStream = new MemoryStream(model3DBinary);
            //FIXME: validator ?
            
            //ImageBuilder.Current.Build(model3DBinary, destStream, new ResizeSettings
            // {
            //    MaxWidth = _mediaSettings.MaximumImageSize,
            //   MaxHeight = _mediaSettings.MaximumImageSize,
            //    Quality = _mediaSettings.DefaultImageQuality
            //});
            return destStream.ToArray();
        }
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets a value indicating whether the images should be stored in data base.
        /// </summary>
        public virtual bool StoreInDb
        {
            get
            {
                return _settingService.GetSettingByKey("Media.Images.StoreInDB", true);
            }
            set
            {
                //check whether it's a new value
                if (this.StoreInDb != value)
                {
                    //save the new setting value
                    _settingService.SetSetting("Media.Images.StoreInDB", value);

                    //update all picture objects
                    var pictures = this.GetModel3Ds(0, int.MaxValue);
                    foreach (var picture in pictures)
                    {
                        var pictureBinary = LoadModel3DBinary(picture, !value);

                        //delete from file system
                        if (value)
                            DeleteModel3DOnFileSystem(picture);

                        //just update a picture (all required logic is in UpdatePicture method)
                        UpdateModel3D(picture.Id,
                                      pictureBinary,
                                      picture.MimeType,
                                      picture.SeoFilename,
                                      true,
                                      false);
                        //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown when "moving" pictures
                    }
                }
            }
        }

        #endregion
    }
}
