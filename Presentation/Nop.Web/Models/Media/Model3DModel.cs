﻿using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Media
{
    public partial class Model3DModel : BaseNopModel
    {
        public string ImageUrl { get; set; }

        public string FullSizeImageUrl { get; set; }

        public string Title { get; set; }

        public string AlternateText { get; set; }
    }
}