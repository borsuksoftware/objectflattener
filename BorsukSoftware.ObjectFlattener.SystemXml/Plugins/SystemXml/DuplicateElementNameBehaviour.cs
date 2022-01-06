namespace BorsukSoftware.ObjectFlattener.Plugins.SystemXml
{
    /// <summary>
    /// Enum detailing how the flattener should behave if there are multiple elements with the same name
    /// </summary>
    public enum DuplicateElementNameBehaviour
    {
        /// <summary>
        /// Any duplicate child elements with the same name will simply be reported as is
        /// </summary>
        ReportAsIs,

        /// <summary>
        /// Any element with a name clash would be reported as 'origName[idx]' (zero based)
        /// </summary>
        AppendIndexWithinDuplicateCollectionZeroBased,

        /// <summary>
        /// Any element with a name clash would be reported as 'origName[idx]' (1 based)
        /// </summary>
        AppendIndexWithinDuplicateCollectionOneBased,

        /// <summary>
        /// Any elements with clashing names would be skipped and not reported
        /// </summary>
        Skip,

        /// <summary>
        /// If there are any duplicates then an exception will be thrown
        /// </summary>
        Throw
    }
}
