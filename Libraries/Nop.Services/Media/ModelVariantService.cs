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
using Nop.Core.Caching;

namespace Nop.Services.Media
{
    /// <summary>
    /// Model3D service
    /// </summary>
    public partial class ModelVariantService : IModelVariantService
    {

        #region Constants
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTMODELVARIANTS_PATTERN_KEY = "Nop.productmodelvariant.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTMODELVARIANTMAPPINGS_PATTERN_KEY = "Nop.productmodelvariantmapping.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTMODELVARIANTVALUES_PATTERN_KEY = "Nop.productmodelvariantvalue.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTMODELVARIANTCOMBINATIONS_PATTERN_KEY = "Nop.productmodelvariantecombination.";

        #endregion


        #region Fields

        private static readonly object s_lock = new object();

        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

        private readonly IRepository<ModelVariant> _modelVariantRepository;
        //private readonly IRepository<ProductModel3D> _productModel3DRepository;
        private readonly IRepository<ProductModelVariant> _productModelVariantRepository;
      

        //private readonly ISettingService _settingService;
        //private readonly IWebHelper _webHelper;
        //private readonly ILogger _logger;
        //private readonly IEventPublisher _eventPublisher;
        //private readonly MediaSettings _mediaSettings;

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
        public ModelVariantService(IRepository<ModelVariant> modelVariantRepository, IRepository<ProductModelVariant> productModelVariantRepository)
        {
            this._modelVariantRepository = modelVariantRepository;
            this._productModelVariantRepository = productModelVariantRepository;
        }

        #endregion


        #region CRUD methods

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        public virtual ModelVariant GetModelVariantById(int modelVariantId)
        {
            if (modelVariantId == 0)
                return null;

            return _modelVariantRepository.GetById(modelVariantId);
        }


        public virtual string Test()
        {
            return "test test test, toto neni test test test";
        }

        //TODO: -> pridat insert ; pridat delete ; pridat udpate ?

        /// <summary>
        /// Deletes a picture
        /// </summary>
        /// <param name="picture">Picture</param>
        /*public virtual void DeleteModel3D(Model3D model3D)
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
        }*/

        /// <summary>
        /// Gets a collection of model variants
        /// </summary>
        /// <returns>List of model variants</returns>
        public virtual IList<ModelVariant> GetModelVariants()
        {
            var query = from p in _modelVariantRepository.Table
                       orderby p.Id
                       select p;
            return query.ToList();
        }


        /// <summary>
        /// Gets model variants by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Model variants</returns>
        public virtual IList<ModelVariant> GetModelVariantsByProductId(int productId)
        {
            if (productId == 0)
                return new List<ModelVariant>();


            var query = from p in _modelVariantRepository.Table
                        join pp in _productModelVariantRepository.Table on p.Id equals pp.ModelVariantId
                        orderby pp.ModelVariantId
                        where pp.ProductId == productId
                        select p;

            return query.ToList();
        }


        /// <summary>
        /// Inserts a product model variant
        /// </summary>
        /// <param name="productAttribute">Product model variant</param>
        public void InsertProductModelVariant(ProductModelVariant productModelVariant)
        {
            if (productModelVariant == null)
                throw new ArgumentNullException("productModelVariant");

            _productModelVariantRepository.Insert(productModelVariant);

            //cache TODO: fix!!
            //_cacheManager.RemoveByPattern(PRODUCTMODELVARIANTS_PATTERN_KEY);
            //_cacheManager.RemoveByPattern(PRODUCTMODELVARIANTMAPPINGS_PATTERN_KEY);
            //_cacheManager.RemoveByPattern(PRODUCTMODELVARIANTVALUES_PATTERN_KEY);
            //_cacheManager.RemoveByPattern(PRODUCTMODELVARIANTCOMBINATIONS_PATTERN_KEY);

            //event notification
            //_eventPublisher.EntityInserted(productModelVariant);
        }

        /// <summary>
        /// Deletes a product model variant
        /// </summary>
        /// <param name="picture">Product model variant</param>
        public void DeleteProductModelVariant(ProductModelVariant productModelVariant)
        {
            _productModelVariantRepository.Delete(productModelVariant);

            //cache TODO: fix!!
        }


        /// <summary>
        /// Gets peoduct model variants by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Model variants</returns>
        public IList<ProductModelVariant> GetProductModelVariantsByProductId(int productId)
        {
            if (productId == 0)
                return null;

            var query = from p in _productModelVariantRepository.Table
                        orderby p.Id
                        where p.ProductId == productId
                        select p;

            return query.ToList();
        }



        /// <summary>
        /// Gets product model variant
        /// </summary>
        //// <param name="pictureId">Product model variant identifier</param>
        /// <returns>Model variants</returns>
        public ProductModelVariant GetProductModelVariantById(int productModelVariantId)
        {
            if (productModelVariantId == 0)
                return null;
            return _productModelVariantRepository.GetById(productModelVariantId);
        }

        #endregion  
    }
}
