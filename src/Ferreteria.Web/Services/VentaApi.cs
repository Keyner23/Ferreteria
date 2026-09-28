using Ferreteria.Shared.Dtos.Ventas;
using Ferreteria.Web.Services;
using System.Net.Http.Json;

namespace Ferreteria.Web.Services
{
    public interface IVentaApi
{
    Task<VentaDto> ComprarAsync(VentaRequest pedido);
    Task<List<VentaDto>> MisComprasAsync();
    Task<List<VentaDto>> ObtenerTodasAsync();
    Task<VentaDto?> ObtenerPorIdAsync(int id);
}

public class VentaApi : IVentaApi
{
    private readonly HttpClient _http;

    public VentaApi(HttpClient http)
    {
        _http = http;
    }

    public async Task<VentaDto> ComprarAsync(VentaRequest pedido)
    {
        var respuesta = await _http.PostAsJsonAsync("api/Ventas", pedido);

        return await respuesta.LeerAsync<VentaDto>();
    }

    public async Task<List<VentaDto>> MisComprasAsync()
    {
        var respuesta = await _http.GetAsync("api/Ventas/mis-compras");

        return await respuesta.LeerAsync<List<VentaDto>>();
    }

    public async Task<List<VentaDto>> ObtenerTodasAsync()
    {
        var respuesta = await _http.GetAsync("api/Ventas");

        return await respuesta.LeerAsync<List<VentaDto>>();
    }

    public async Task<VentaDto?> ObtenerPorIdAsync(int id)
    {
        var respuesta = await _http.GetAsync($"api/Ventas/{id}");

        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        return await respuesta.LeerAsync<VentaDto>();
    }
}
}
