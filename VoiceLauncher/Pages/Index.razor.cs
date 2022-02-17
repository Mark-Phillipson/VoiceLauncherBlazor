using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using VoiceLauncher;
using VoiceLauncher.Shared;
using Blazored.Toast;
using Blazored.Toast.Services;
using Blazored.Modal;
using Blazored.Modal.Services;
using DataAccessLibrary;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using Blazored.Typeahead;

namespace VoiceLauncher.Pages
{
    public partial class Index
    {
    }
}