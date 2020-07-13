// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: SemanticVersion.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TA.Utils.Core
    {
    /// <summary>
    ///     <para>
    ///         Implements parsing, storage and comparison of semantic versions as defined at
    ///         http://semver.org/.
    ///     </para>
    ///     <para>
    ///         Semantic Versions have a very specific and somewhat counter-intuitive order of precedence.
    ///         Comparison begins with the major version and proceeds to the minor version, patch,
    ///         prerelease tag and build metadata tag. The order of precedence is always returned as soon
    ///         as it can be determined.
    ///     </para>
    ///     <para>
    ///         If order cannot be determined from the major, minor and patch versions, then comparison
    ///         proceeds to the prerelease tag and then the build metadata tag. These fields can contain
    ///         multiple segments separated by the '.' character. each dot-separated segment is considered
    ///         separately and where possible is converted to an integer, so that <c>beta.9</c> sorts
    ///         before <c>beta.10</c>.
    ///     </para>
    ///     <para>
    ///         Note that any version with a prerelease tag sorts lower than the same version without a
    ///         prerelease tag. Put another way: a release version is greater than a prerelease version.
    ///     </para>
    ///     <para>
    ///         The specification states that build metadata should be ignored when determining precedence.
    ///         That doesn't seem like a very sensible approach to us, since builds have to appear in some
    ///         sort of order and 'random' didn't strike us as an amazingly useful outcome. Therefore we
    ///         have chosen to deviate from the specification and include it as the last item in the list
    ///         of comparisons when determining the collation sequence. We treat the build metadata in a
    ///         similar way to the prerelease tag, giving it the lowest precedence but nonetheless yielding
    ///         a more deterministic result when comparing and sorting semantic versions. Build metadata is
    ///         NOT considered when determining equality.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     This class was inspired by Michael F. Collins and based on his blog article at
    ///     http://www.michaelfcollins3.me/blog/2013/01/23/semantic_versioning_dotnet.html. For guidance on
    ///     the meaning and rules of semantic versioning, please see https://semver.org/
    /// </remarks>
    public sealed class SemanticVersion : IEquatable<SemanticVersion>, IComparable, IComparable<SemanticVersion>
        {
        internal const string SemanticVersionPattern =
            @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<prerelease>[A-Za-z0-9\-\.]+))?(\+(?<build>[A-Za-z0-9\-\.]+))?$";

        private const string AllDigitsPattern = @"^[0-9]+$";

        private static readonly Regex SemanticVersionRegex = new Regex(
            SemanticVersionPattern,
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex AllDigitsRegex = new Regex(
            AllDigitsPattern,
            RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        ///     Initializes a new instance of the <see cref="SemanticVersion" /> class from a version encoded
        ///     in a string.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <exception cref="System.ArgumentException">version</exception>
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public SemanticVersion(string version)
            {
            Contract.Requires(!string.IsNullOrEmpty(version));
            Contract.Ensures(MajorVersion >= 0);
            Contract.Ensures(MinorVersion >= 0);
            Contract.Ensures(PatchVersion >= 0);
            Contract.Ensures(PrereleaseVersion != null);
            Contract.Ensures(BuildVersion != null);

            var match = SemanticVersionRegex.Match(version);
            if (!match.Success)
                {
                string message = $"The version number '{version}' is not a valid semantic version number.";
                throw new ArgumentException(message, nameof(version));
                }

            MajorVersion = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);
            MinorVersion = int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture);
            PatchVersion = int.Parse(match.Groups["patch"].Value, CultureInfo.InvariantCulture);
            PrereleaseVersion = match.Groups["prerelease"].Success
                ? match.Groups["prerelease"].Value.AsMaybe()
                : Maybe<string>.Empty;
            BuildVersion = match.Groups["build"].Success
                ? match.Groups["build"].Value.AsMaybe()
                : Maybe<string>.Empty;
            }

        /// <summary>Initializes a new instance of the <see cref="T:TA.Utils.Core.SemanticVersion" /> class.</summary>
        /// <param name="majorVersion">The major version.</param>
        /// <param name="minorVersion">The minor version.</param>
        /// <param name="patchVersion">The patch version.</param>
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public SemanticVersion(int majorVersion, int minorVersion, int patchVersion)
            {
            Contract.Requires(majorVersion >= 0);
            Contract.Requires(minorVersion >= 0);
            Contract.Requires(patchVersion >= 0);
            Contract.Ensures(MajorVersion >= 0);
            Contract.Ensures(MinorVersion >= 0);
            Contract.Ensures(PatchVersion >= 0);
            Contract.Ensures(PrereleaseVersion != null);
            Contract.Ensures(BuildVersion != null);

            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            PatchVersion = patchVersion;
            PrereleaseVersion = Maybe<string>.Empty;
            BuildVersion = Maybe<string>.Empty;
            }

        /// <summary>Gets the build version, if any.</summary>
        /// <value>The build version.</value>
        public Maybe<string> BuildVersion { get; }

        /// <summary>Gets the major version.</summary>
        /// <value>The major version.</value>
        public int MajorVersion { get; }

        /// <summary>Gets the minor version.</summary>
        /// <value>The minor version.</value>
        public int MinorVersion { get; }

        /// <summary>Gets the patch version.</summary>
        /// <value>The patch version.</value>
        public int PatchVersion { get; }

        /// <summary>Gets the prerelease version, if any.</summary>
        /// <value>The prerelease version.</value>
        public Maybe<string> PrereleaseVersion { get; }

        [ContractInvariantMethod]
        private void ObjectInvariant()
            {
            Contract.Invariant(MajorVersion >= 0);
            Contract.Invariant(MinorVersion >= 0);
            Contract.Invariant(PatchVersion >= 0);
            Contract.Invariant(BuildVersion != null);
            Contract.Invariant(PrereleaseVersion != null);
            }

        /// <summary>Returns a semantic version string.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
            {
            Contract.Ensures(Contract.Result<string>() != null);
            var builder = new StringBuilder();
            builder.Append($"{MajorVersion}.{MinorVersion}.{PatchVersion}");
            if (PrereleaseVersion.Any())
                builder.Append($"-{PrereleaseVersion.Single()}");
            if (BuildVersion.Any())
                builder.Append($"+{BuildVersion.Single()}");
            return builder.ToString();
            }

        /// <summary>Tests a string to determine whether it is a valid semantic version string.</summary>
        /// <param name="candidate">The candidate string to be examined.</param>
        /// <returns>
        ///     <c>true</c> if the specified candidate is a valid semantic version; otherwise,
        ///     <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public static bool IsValid(string candidate)
            {
            return SemanticVersionRegex.IsMatch(candidate);
            }

        #region Equality members
        /// <summary>
        ///     Indicates whether this semantic version is equal to another semantic version. For two versions
        ///     to be equal, they must have the same Major, Minor and Patch versions and the Prerelease tag
        ///     must match. Build metadata is not considered part of the version and is not checked.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise,
        ///     false.
        /// </returns>
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public bool Equals(SemanticVersion other)
            {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            // Note that PrereleaseVersion is Maybe<string>
            return MajorVersion == other.MajorVersion
                   && MinorVersion == other.MinorVersion
                   && PatchVersion == other.PatchVersion
                   && PrereleaseVersion.SequenceEqual(other.PrereleaseVersion);
            }

        /// <summary>
        ///     Determines whether the specified (possibly null) <see cref="T:System.Object" /> is equal to
        ///     this instance.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance;
        ///     otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public override bool Equals(object other)
            {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other is SemanticVersion version && Equals(version);
            }

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like
        ///     a hash table.
        /// </returns>
        public override int GetHashCode()
            {
            /*
             * [TPL 2020-07] A requirement for hash codes is that items considered to be equal should have
             * the same hash. Since SemVer does not treat build metadata as significant, then we should
             * not include it in the hash code.
             */
            unchecked
                {
                int hashCode = MajorVersion; //MaybeHashCode(BuildVersion);
                //hashCode = (hashCode * 397) ^ MajorVersion;
                hashCode = (hashCode * 397) ^ MinorVersion;
                hashCode = (hashCode * 397) ^ PatchVersion;
                hashCode = (hashCode * 397) ^ MaybeHashCode(PrereleaseVersion);
                return hashCode;
                }
            }

        private int MaybeHashCode(Maybe<string> item)
            {
            return item.Any() ? item.Single().GetHashCode() : string.Empty.GetHashCode();
            }

        /// <summary>Tests two semantic versions for equality.</summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        ///     <c>true</c> if the two versions are equal according to the rules for semantic versioning.
        ///     otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="Equals(TA.Utils.Core.SemanticVersion)" />
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public static bool operator ==(SemanticVersion left, SemanticVersion right)
            {
            return Equals(left, right);
            }

        /// <summary>Tests two semantic versions for inequality.</summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument.</param>
        /// <returns>
        ///     <c>true</c> if the two versions are unequal according to the rules for semantic versioning.
        ///     otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="Equals(TA.Utils.Core.SemanticVersion)" />
        /// <remarks>
        ///     For guidance on the meaning and rules of semantic versioning, please see
        ///     https://semver.org/
        /// </remarks>
        public static bool operator !=(SemanticVersion left, SemanticVersion right)
            {
            return !Equals(left, right);
            }
        #endregion Equality members

        #region IComparable members
        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that
        ///     indicates whether the current instance precedes, follows, or occurs in the same position in the
        ///     sort order as the comparison object.
        /// </summary>
        /// <param name="comparison">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has
        ///     these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Value</term><term>Meaning</term>
        ///         </listheader>
        ///         <item>
        ///             <description>Less than zero</description>
        ///             <description>This instance precedes <paramref name="comparison" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <description>Zero</description>
        ///             <description>
        ///                 This instance occurs in the same position in the sort order as
        ///                 <paramref name="comparison" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>Greater than zero</description>
        ///             <description>This instance succeeds <paramref name="comparison" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="comparison" /> is not the same type as
        ///     this instance.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="comparison" /> is <c>null</c>.</exception>
        public int CompareTo(object comparison)
            {
            if (ReferenceEquals(comparison, null))
                throw new ArgumentNullException(nameof(comparison));
            var otherVersion = comparison as SemanticVersion;
            if (otherVersion == null)
                throw new ArgumentException("Must be an instance of SemanticVersion.", nameof(comparison));
            return CompareTo(otherVersion);
            }

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that
        ///     indicates whether the current instance precedes, follows, or occurs in the same position in the
        ///     sort order as the <paramref name="comparison" /> object.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Semantic Versions have a very specific and somewhat counterintuitive order of precedence.
        ///         Comparison begins with the major version and proceeds to the minor version, patch,
        ///         prerelease tag and build metadata tag. The order of precedence is always returned as soon
        ///         as it can be determined.
        ///     </para>
        ///     <para>
        ///         If order cannot be determined from the major, minor and patch versions, then comparison
        ///         proceeds to the prerelease tag and then the build metadata tag. These fields can contain
        ///         multiple segments separated by the '.' character. each dot-separated segment is considered
        ///         separately and where possible is converted to an integer, so that <c>beta.9</c> sorts
        ///         before <c>beta.10</c>.
        ///     </para>
        ///     <para>
        ///         Note that any version with a prerelease tag sorts lower than the same version without a
        ///         prerelease tag. Put another way: a release version is greater than a prerelease version.
        ///     </para>
        ///     <para>
        ///         The specification states that build metadata should be ignored when determining precedence.
        ///         That doesn't seem like a very sensible approach to us, since builds have to appear in some
        ///         sort of order and 'random' didn't strike us as an amazingly useful outcome. Therefore we
        ///         have chosen to deviate from the specification and include it as the last item in the list
        ///         of comparisons when determining the collation sequence. We treat the build metadata in a
        ///         similar way to the prerelease tag, giving it the lowest precedence but nonetheless yielding
        ///         a more deterministic result when comparing and sorting semantic versions.
        ///     </para>
        /// </remarks>
        /// <param name="comparison">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(SemanticVersion comparison)
            {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));
            if (ReferenceEquals(this, comparison))
                return 0;
            int result = MajorVersion.CompareTo(comparison.MajorVersion);
            if (result != 0)
                return result;
            result = MinorVersion.CompareTo(comparison.MinorVersion);
            if (result != 0)
                return result;
            result = PatchVersion.CompareTo(comparison.PatchVersion);
            if (result != 0)
                return result;
            result = ComparePrereleaseVersions(PrereleaseVersion, comparison.PrereleaseVersion);
            if (result != 0)
                return result;
            return CompareBuildVersions(BuildVersion, comparison.BuildVersion);
            }

        private static int CompareBuildVersions(Maybe<string> leftVersion, Maybe<string> rightVersion)
            {
            if (leftVersion.None && rightVersion.None)
                return 0; // equal if both absent
            if (leftVersion.Any() && rightVersion.None)
                return 1;
            if (leftVersion.None && rightVersion.Any())
                return -1;
            int result = CompareSegmentBySegment(leftVersion.Single(), rightVersion.Single());
            return result;
            }

        private static int CompareSegmentBySegment(string left, string right)
            {
            Contract.Requires(!string.IsNullOrEmpty(left));
            Contract.Requires(!string.IsNullOrEmpty(right));
            int result;
            var dotDelimiter = new[] {'.'};
            var leftSegments = left.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries);
            var rightSegments = right.Split(dotDelimiter, StringSplitOptions.RemoveEmptyEntries);
            int longest = Math.Max(leftSegments.Length, rightSegments.Length);
            for (int i = 0; i < longest; i++)
                {
                // If we've run out of segments on either side, that side is the lesser version.
                if (i >= leftSegments.Length)
                    return -1;
                if (i >= rightSegments.Length)
                    return 1;

                // Compare the next segment to see if we can determine inequality.
                result = CompareSegmentPreferNumericSort(leftSegments[i], rightSegments[i]);
                if (result != 0)
                    return result;

                // We haven't determined inequality, so we have to go around to the next segment.
                }

            // If we've run out of segments on both sides, they must be equal by definition.
            return 0;
            }

        private static int CompareSegmentPreferNumericSort(string left, string right)
            {
            if (AllDigitsRegex.IsMatch(left) && AllDigitsRegex.IsMatch(right))
                {
                int value1 = int.Parse(left, CultureInfo.InvariantCulture);
                int value2 = int.Parse(right, CultureInfo.InvariantCulture);
                return value1.CompareTo(value2);
                }

            return string.Compare(left, right, StringComparison.Ordinal);
            }

        private static int ComparePrereleaseVersions(Maybe<string> leftVersion, Maybe<string> rightVersion)
            {
            // If the prerelease segment is absent in both instances, then they are considered equal.
            if (leftVersion.None && rightVersion.None)
                return 0;

            // By definition, a prerelease version is less than the absence of a prerelease version - this is a bit counterintuitive.
            if (leftVersion.Any() && rightVersion.None)
                return -1;
            if (leftVersion.None && rightVersion.Any())
                return 1;
            int result = CompareSegmentBySegment(leftVersion.Single(), rightVersion.Single());
            return result;
            }

        /// <summary>
        ///     Determines whether the left version is less than the right version,
        ///     according to the collation rules for semantic versions.</summary>
        /// <param name="left">The left version operand.</param>
        /// <param name="right">The right version operand.</param>
        /// <returns><c>true</c> if the left version is less than the right version.</returns>
        /// <seealso cref="CompareTo(TA.Utils.Core.SemanticVersion)"/>
        public static bool operator <(SemanticVersion left, SemanticVersion right)
            {
            Contract.Requires(null != left);
            Contract.Requires(null != right);
            return left.CompareTo(right) < 0;
            }

        /// <summary>
        ///     Determines whether the left version is greater than the right version,
        ///     according to the collation rules for semantic versions.</summary>
        /// <param name="left">The left version operand.</param>
        /// <param name="right">The right version operand.</param>
        /// <returns><c>true</c> if the left version is greater than the right version.</returns>
        /// <seealso cref="CompareTo(TA.Utils.Core.SemanticVersion)"/>

        public static bool operator >(SemanticVersion left, SemanticVersion right)
            {
            Contract.Requires(null != left);
            Contract.Requires(null != right);
            return left.CompareTo(right) > 0;
            }

        /// <summary>
        ///     Determines whether the left version is less than or equal to the right version,
        ///     according to the collation and equality rules for semantic versions.</summary>
        /// <param name="left">The left version operand.</param>
        /// <param name="right">The right version operand.</param>
        /// <returns><c>true</c> if the left version is less than or equal to the right version.</returns>
        /// <seealso cref="CompareTo(TA.Utils.Core.SemanticVersion)"/>
        /// <seealso cref="Equals(TA.Utils.Core.SemanticVersion)"/>
        public static bool operator <=(SemanticVersion left, SemanticVersion right)
            {
            Contract.Requires(null != left);
            Contract.Requires(null != right);
            if (left.Equals(right)) return true;
            return left.CompareTo(right) < 0;
            }

        /// <summary>
        ///     Determines whether the left version is greater than or equal to the right version,
        ///     according to the collation and equality rules for semantic versions.</summary>
        /// <param name="left">The left version operand.</param>
        /// <param name="right">The right version operand.</param>
        /// <returns><c>true</c> if the left version is greater than or equal to the right version.</returns>
        /// <seealso cref="CompareTo(TA.Utils.Core.SemanticVersion)"/>
        /// <seealso cref="Equals(TA.Utils.Core.SemanticVersion)"/>
        public static bool operator >=(SemanticVersion left, SemanticVersion right)
            {
            Contract.Requires(null != left);
            Contract.Requires(null != right);
            if (left.Equals(right)) return true;
            return left.CompareTo(right) > 0;
            }
        #endregion
        }
    }