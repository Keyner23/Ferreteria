
using Blazored.LocalStorage;
using Ferreteria.Web;
using Ferreteria.Web.Auth;
using Ferreteria.Web.Carrito;
using Ferreteria.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<ICategoriaApi, CategoriaApi>();
builder.Services.AddScoped<IProductoApi, ProductoApi>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IVentaApi, VentaApi>();
builder.Services.AddScoped<IClienteApi, ClienteApi>();



// El proveedor concreto y su registro como AuthenticationStateProvider:
// las páginas piden la clase concreta para llamar a MarcarComoAutenticadoAsync,
// y Blazor pide la abstracta para saber quién está conectado. Deben ser la MISMA instancia.
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthorizationHandler>();

// HttpClient con el handler: toda petición sale con el token puesto.
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
});

builder.Services.AddScoped<IAuthApi, AuthApi>();

await builder.Build().RunAsync();