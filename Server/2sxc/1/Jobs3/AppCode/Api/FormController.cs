// Add namespaces to enable security in Oqtane & Dnn despite the differences
#if NETCOREAPP
using Microsoft.AspNetCore.Authorization; // .net core [AllowAnonymous] & [Authorize]
using Microsoft.AspNetCore.Mvc;           // .net core [HttpGet] / [HttpPost] etc.
#else
// 2sxclint:disable:no-dnn-namespaces 2sxclint:disable:no-web-namespaces
using System.Web.Http;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Sxc.WebApi;

// WIP
using System.Text.Json;
using System.Text.Json.Nodes;
using AppCode.Mail;
using AppCode.Data;

[AllowAnonymous]	// define that all commands can be accessed without a login
[JsonFormatter]   // Use modern JSON formatter
public class FormController : Custom.Hybrid.ApiTyped
{
  [HttpPost]
  public void ProcessForm([FromBody] SaveRequest contactFormRequest)
  {

    var wrapLog = Log.Call(useTimer: true);
    // 0. Pre-Check - validate recaptcha if enabled in the current object (the form configuration)
    // var appSettings = As<AppSettings>(App.Settings);
    // if (appSettings.Recaptcha)
    // {
    //   Log.Add("checking Recaptcha");
    //   GetService<Recaptcha>().Validate(contactFormRequest.Recaptcha);
    // }

    // Copy the data into a new variable, as only this will be sent per Mail and the Other Data is need to Save in the 2sxc
    var fieldsFormRequest = new Dictionary<string, object>(contactFormRequest.Fields, StringComparer.OrdinalIgnoreCase);
    // Same the TechnicalValues
    Dictionary<string, object> formTechnicalValues = new Dictionary<string, object>();

    // 1. add IP / host, and save all fields
    // if you add fields to your content-type, just make sure they are
    // in the request with the correct name, they will be added automatically
    formTechnicalValues["Timestamp"] = DateTime.Now;
    // Add the SenderIP in case we need to track down abuse
#if NETCOREAPP
      formTechnicalValues["SenderIP"] = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
#else
    formTechnicalValues["SenderIP"] = System.Web.HttpContext.Current.Request.UserHostAddress;
#endif
    // Add the ModuleId to assign each sent form to a specific module
    formTechnicalValues["ModuleId"] = MyContext.Module.Id;
    // add raw-data, in case the content-type has a "RawData" field
    formTechnicalValues["RawData"] = CreateRawDataEntry(contactFormRequest.Fields);
    // add Title (if non given), in case the content-type would benefit of an automatic title
    var addTitle = !contactFormRequest.Fields.ContainsKey("Title");
    if (addTitle) contactFormRequest.Fields.Add("Title", "Form " + DateTime.Now.ToString("s"));
    formTechnicalValues["Title"] = contactFormRequest.Fields["Title"];

    // Automatically full-save each request into a system-protocol content-type
    // This helps to debug or find submissions in case something wasn't configured right
    Log.Add("Save data to SystemProtocol in case we ever need to see what was submitted");
    App.Data.Create("SystemProtocol", formTechnicalValues);

    // Add guid to identify entity after saving (because we need to find it afterwards)
    var formDataEntity = App.Data.Create("Registrations", contactFormRequest.Fields);

    Log.Add("Save data to content type");
    var files = new List<ToSic.Sxc.Adam.IFile>();

    // Save files to Adam
    if (contactFormRequest.Files != null)
    {
      foreach (var fileObj in contactFormRequest.Files)
      {
        files.Add(SaveInAdam(
          stream: new MemoryStream(fileObj.Contents),
          fileName: fileObj.Name,
          contentType: "Registrations",
          guid: formDataEntity.EntityGuid,
          field: "ApplicationFile"));
      }
    }
    else
    {
      Log.Add("No files found to save");
    }

    // sending Mails
    GetService<Mail>().sendMails(fieldsFormRequest, files);
    wrapLog("ok");
  }

  private object CreateRawDataEntry(Dictionary<string, object> formRequest)
  {
    var data = new Dictionary<string, object>(formRequest, StringComparer.OrdinalIgnoreCase);
    data.Remove("Files");
    return Kit.Json.ToJson(data);
  }
}

public class SaveRequest
{
  public List<FileUpload> Files { get; set; }

  public Dictionary<string, object> Fields { get; set; }
  public Dictionary<string, string> Terms { get; set; }
  public string Recaptcha { get; set; }
  public string CustomerMails { get; set; }
}
public class FileUpload
{
  public string Field { get; set; }
  public string Name { get; set; }
  public string Encoded { get; set; }
  public byte[] Contents { get { return System.Convert.FromBase64String(Encoded.Split(',')[1]); } }
}