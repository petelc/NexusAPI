using Nexus.API.Core.ContributorAggregate;
using Vogen;

namespace Nexus.API.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
internal partial class VogenEfCoreConverters;
