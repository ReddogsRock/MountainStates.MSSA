using ToSic.Razor.Blade;
using AppCode.Data;
using System.Collections.Generic;
using System.Linq;

namespace AppCode.Razor
{
    /// <summary>
    /// Base Class for Razor Views which have a typed App but don't use the Model or use the typed MyModel.
    /// </summary>
    public abstract class DetailRazor : AppRazor
  {
    /// <summary>
    /// Get JsonLd for a JobPosting
    /// </summary>
    public object GetJsonLdForDetailJob(Job job, EmploymentType[] employmentTypes )
    {

    var AppSet = App.Settings;
    var AppRes = App.Resources;

    var jsonDescription = Tag.Strong(job.Description) + "<br/><br/>" + job.Intro + "<br/><br/>" +
                        Tag.Strong(AppRes.TasksHeading) + job.Tasks + Tag.Strong(AppRes.QualificationsHeading) +
                        job.Qualifications + Tag.Strong(AppRes.OurOfferHeading) + job.OurOffer;

      return new Dictionary<string, object> {
            { "@context", "https://schema.org"},
            { "@type", "JobPosting" },
            { "title", job.Name },
            { "description", jsonDescription},
            { "datePosted", job.Date.ToString("s") },
            { "hiringOrganization", new Dictionary<string, object> {
                { "@type", "Organization" },
                { "name", AppSet.Organization },
                { "logo", MyContext.Site.Url.Replace("/job-app", "") + "/Portals/job-app/2sxc/Jobs2/src/2sic-logo-square.png" },
                { "sameAs", MyContext.Site.Url }
            }},
            { "jobLocation", new Dictionary<string, object> {
                {"@type", "Place"},
                { "address", new Dictionary<string, object> {
                    { "@type", "PostalAddress" },
                    { "streetAddress", AppSet.OrganizationStreet },
                    { "addressLocality", AppSet.OrganizationCity },
                    { "addressRegion", AppSet.OrganizationRegion },
                    { "postalCode", AppSet.OrganizationZip },
                    { "addressCountry", AppSet.OrganizationCountry }

                }}
            }},
            { "employmentType", employmentTypes.Select(e => e.Key) },
            { "directApply", true },
            { "identifier", new Dictionary<string, object> {
                { "@type", "PropertyValue" },
                { "name", AppSet.Identifier },
                { "value", job.Id }
            }}
        };
    }
  }

}
