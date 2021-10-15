using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EncurtaWebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class XController : ControllerBase
    {
        private readonly IMemoryCache MemoryCache;
        string chaveCache = "CHAVE_CACHE";
        Random random = new Random();
        string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";        

        public XController(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        [HttpGet]
        public Dictionary<string, string> Get()
        {
            MemoryCache.TryGetValue(chaveCache, out Dictionary<string, string> ListaEncurados);
            return ListaEncurados;
            
        }

        [HttpGet("{value}")]
        public ActionResult Get(string value)
        {
            MemoryCache.TryGetValue(chaveCache, out Dictionary<string, string> ListaEncurados);

            if (!ListaEncurados.ContainsKey(value))
            {
                return BadRequest("Codigo não localizado!");
            }
            return Redirect(ListaEncurados[value]);
        }

        //Retorna o cod Encriptografado
        [HttpPost]
        public ActionResult Post([FromBody] string value)
        {
            //Add validação de https://
            if (string.IsNullOrWhiteSpace(value) || !value.ToLower().Contains("http") || !value.ToLower().Contains("://"))
            {
                return BadRequest("Parametro de entrada inválido!");
            }
           
            if (MemoryCache.TryGetValue(chaveCache, out Dictionary<string, string> ListaEncurados))
            {
                if (ListaEncurados.ContainsValue(value))
                {
                    return Ok(ListaEncurados.FirstOrDefault(_ => _.Value == value).Key);
                }
            }

            bool key = true;
            string chave = string.Empty;
            while (key == true)
            {
                chave = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());

                if (ListaEncurados == null)
                    ListaEncurados = new Dictionary<string, string>();

                if (!ListaEncurados.ContainsKey(chave))
                {
                    ListaEncurados.Add(chave, value);
                    MemoryCache.Set(chaveCache, ListaEncurados);
                    key = false;
                }
            }

            return Ok(chave);
        }

        // DELETE api/<XController>/5
        [HttpDelete("{value}")]
        public void Delete(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                BadRequest("Parametro de entrada inválido!");
            }

            if (MemoryCache.TryGetValue(chaveCache, out Dictionary<string, string> ListaEncurados))
            {

                if (!ListaEncurados.ContainsKey(value) && !ListaEncurados.ContainsValue(value))
                {
                    BadRequest("Parametro de entrada inválido!");
                }
                else
                {
                    if (ListaEncurados.ContainsKey(value))
                    {
                        ListaEncurados.Remove(value);
                    }

                    if (ListaEncurados.ContainsValue(value))
                    {
                        ListaEncurados.Remove(ListaEncurados.FirstOrDefault(_ => _.Value == value).Key);
                    }

                    MemoryCache.Set(chaveCache, ListaEncurados);
                }
            }
        }
    }
}
