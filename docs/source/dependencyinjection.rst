Dependency Injection
====================

Registering the Service
^^^^^^^^^^^^^^^^^^^^^^^

Elm supports Dependency Injection out of the box and it's fairly simple to set up. In this page we'll use an ASP.NET Core WebApi/MVC6 project as an example but this would work with a Console project as well.

In your ``Startup.cs`` file you should have a method to configure services. The default Microsoft scaffolding creates it as ``public void ConfigureServices(IServiceCollection services)``. 

To add Elm simply add this line:

.. code-block:: c#

    services.AddElm<MySqlDriver>(options => options.ConnectionString = Configuration["Data:ConnectionString"]);
    
This assumes you want to use MySQL/MariaDb. Replace ``MySqlDriver`` by ``SQLiteDriver`` or ``SqlServerDriver`` depending on your requirements. This also assumes that your ConnectionString is stored in a JSON file that was provided to the ``ConfigurationBuilder`` in the ``Startup`` method, like this:

.. code-block:: c#

    public IConfigurationRoot Configuration { get; set; }
        public Startup(IHostingEnvironment env)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            configurationBuilder.AddCommandLine(new string[] { });
            Configuration = configurationBuilder.Build();
        }
        
You will of course need to use the relevant namespaces, in our cases they would be:

.. code-block:: c#

    using Folke.Elm;
    using Folke.Elm.Mysql;

Now that the service is registered, let's see how to setup a Controller to use it.

Use the Service
^^^^^^^^^^^^^^^^

In your Controller, simply specify in the constructor that you want an ``IFolkeConnection`` object. Create a Property for it and associate it.

.. code-block:: c#

    [Route("api/[controller]")]
    public class MyController : Controller
    {
        protected readonly IFolkeConnection session;
        public MyController(IFolkeConnection session)
        {
            this.session = session;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Product myProduct = session.Load<Product>(id);
            return Ok(myProduct);
        }
    }    