using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using XperienceCommunity.Sustainability;

[assembly: RegisterObjectType(typeof(SustainabilityPageDataInfo), SustainabilityPageDataInfo.OBJECT_TYPE)]

namespace XperienceCommunity.Sustainability
{
    /// <summary>
    /// Data container class for <see cref="SustainabilityPageDataInfo"/>.
    /// </summary>
    public partial class SustainabilityPageDataInfo : AbstractInfo<SustainabilityPageDataInfo, IInfoProvider<SustainabilityPageDataInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "xperiencecommunity.sustainabilitypagedata";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<SustainabilityPageDataInfo>), OBJECT_TYPE, "XperienceCommunity.SustainabilityPageData", "SustainabilityPageDataID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Sustainability page data ID.
        /// </summary>
        [DatabaseField]
        public virtual int SustainabilityPageDataID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(SustainabilityPageDataID)), 0);
            set => SetValue(nameof(SustainabilityPageDataID), value);
        }


        /// <summary>
        /// Web page item ID.
        /// </summary>
        [DatabaseField]
        public virtual int WebPageItemID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(WebPageItemID)), 0);
            set => SetValue(nameof(WebPageItemID), value);
        }


        /// <summary>
        /// Language name.
        /// </summary>
        [DatabaseField]
        public virtual string LanguageName
        {
            get => ValidationHelper.GetString(GetValue(nameof(LanguageName)), String.Empty);
            set => SetValue(nameof(LanguageName), value);
        }


        /// <summary>
        /// Date created.
        /// </summary>
        [DatabaseField]
        public virtual DateTime DateCreated
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(DateCreated)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(DateCreated), value);
        }


        /// <summary>
        /// Total size.
        /// </summary>
        [DatabaseField]
        public virtual decimal TotalSize
        {
            get => ValidationHelper.GetDecimal(GetValue(nameof(TotalSize)), 0m);
            set => SetValue(nameof(TotalSize), value);
        }


        /// <summary>
        /// Total emissions.
        /// </summary>
        [DatabaseField]
        public virtual double TotalEmissions
        {
            get => ValidationHelper.GetDouble(GetValue(nameof(TotalEmissions)), 0d);
            set => SetValue(nameof(TotalEmissions), value);
        }


        /// <summary>
        /// Carbon rating.
        /// </summary>
        [DatabaseField]
        public virtual string CarbonRating
        {
            get => ValidationHelper.GetString(GetValue(nameof(CarbonRating)), String.Empty);
            set => SetValue(nameof(CarbonRating), value);
        }


        /// <summary>
        /// Resource groups.
        /// </summary>
        [DatabaseField]
        public virtual string ResourceGroups
        {
            get => ValidationHelper.GetString(GetValue(nameof(ResourceGroups)), String.Empty);
            set => SetValue(nameof(ResourceGroups), value);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="SustainabilityPageDataInfo"/> class.
        /// </summary>
        public SustainabilityPageDataInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SustainabilityPageDataInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SustainabilityPageDataInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}