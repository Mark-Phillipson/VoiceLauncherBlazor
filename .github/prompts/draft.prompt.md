Can we create a new Blazor component in the Razor class library to display the talon lists and their values?

 we would like to be able to filter by the talon list name and the values.

  there is no need to add any CRUD  just need to be able to filter them.

  The new component should be called TalonListViewer.razor and should be added to the RazorClassLibrary project.

   initially can we create the component so it is as a page that can be navigated to from the main menu or a button on a different component.

    the page can be the following url /talon-lists

     the data resides in a table called TalonLists

       we will need a data service and a repository to access the data which will need to be included in the dependency injection pipeline for the razor class library.