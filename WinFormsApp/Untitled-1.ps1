---------------------------
Error
---------------------------
System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation.

 ---> System.InvalidOperationException: No service for type 'Microsoft.Extensions.Configuration.IConfigurationRoot' has been registered.

   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)

   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)

   at WinFormsApp.MainForm.<>c.<.ctor>b__0_0(IServiceProvider provider) in C:\Users\MPhil\source\repos\VoiceLauncherBlazor\WinFormsApp\MainForm.cs:line 24

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSiteMain(ServiceCallSite callSite, TArgument argument)

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitRootCache(ServiceCallSite callSite, RuntimeResolverContext context)

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScopeCache(ServiceCallSite callSite, RuntimeResolverContext context)

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteVisitor`2.VisitCallSite(ServiceCallSite callSite, TArgument argument)

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.DynamicServiceProviderEngine.<>c__DisplayClass2_0.<RealizeService>b__0(ServiceProviderEngineScope scope)

   at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)

   at Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceProviderEngineScope.GetService(Type serviceType)

   at lambda_method1(Closure, IServiceProvider, Object[])

   at Microsoft.EntityFrameworkCore.Internal.DbContextFactorySource`1.<>c__DisplayClass4_0.<CreateActivator>b__1(IServiceProvider p, DbContextOptions`1 _)

   at Microsoft.EntityFrameworkCore.Internal.DbContextFactory`1.CreateDbContext()

   at DataAccessLibrary.Services.CategoryService.GetCategoryAsync(Int32 categoryId) in C:\Users\MPhil\source\repos\VoiceLauncherBlazor\DataAccessLibrary\Services\CategoryService.cs:line 67

   at RazorClassLibrary.Pages.CustomIntelliSenses.LoadData() in C:\Users\MPhil\source\repos\VoiceLauncherBlazor\RazorClassLibrary\Pages\CustomIntelliSenses.razor.cs:line 152

   at RazorClassLibrary.Pages.CustomIntelliSenses.OnInitializedAsync() in C:\Users\MPhil\source\repos\VoiceLauncherBlazor\RazorClassLibrary\Pages\CustomIntelliSenses.razor.cs:line 105

   at Microsoft.AspNetCore.Components.ComponentBase.RunInitAndSetParametersAsync()

   at Microsoft.AspNetCore.Components.WebView.IpcSender.<>c__DisplayClass12_0.<NotifyUnhandledException>b__1()

   at Microsoft.AspNetCore.Components.WebView.WindowsForms.WindowsFormsDispatcher.InvokeAsync(Action workItem)

   at Microsoft.AspNetCore.Components.WebView.WindowsForms.WindowsFormsDispatcher.<>c.<.cctor>b__8_0(Exception exception)

   at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)

   at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)

   --- End of inner exception stack trace ---

   at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)

   at System.Reflection.MethodBaseInvoker.InvokeWithOneArg(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)

   at System.Delegate.DynamicInvokeImpl(Object[] args)

   at System.Windows.Forms.Control.InvokeMarshaledCallbackDo(ThreadMethodEntry tme)

   at System.Windows.Forms.Control.InvokeMarshaledCallbackHelper(Object obj)

   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)

--- End of stack trace from previous location ---

   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)

   at System.Windows.Forms.Control.InvokeMarshaledCallbacks()

   at System.Windows.Forms.Control.WndProc(Message& m)

   at System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)

   at System.Windows.Forms.NativeWindow.Callback(HWND hWnd, MessageId msg, WPARAM wparam, LPARAM lparam)

   at Windows.Win32.PInvoke.DispatchMessage(MSG* lpMsg)

   at System.Windows.Forms.Application.ComponentManager.Microsoft.Office.IMsoComponentManager.FPushMessageLoop(UIntPtr dwComponentID, msoloop uReason, Void* pvLoopData)

   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(msoloop reason, ApplicationContext context)

   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(msoloop reason, ApplicationContext context)

   at WinFormsApp.Program.Main() in C:\Users\MPhil\source\repos\VoiceLauncherBlazor\WinFormsApp\Program.cs:line 27
---------------------------
OK   
---------------------------
