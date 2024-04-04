using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NeoCheck.FraudPrevention.API
{
    [ApiController]
    [Route("[controller]")]
    public class FraudPreventionController : ControllerBase
    {

        private readonly ILogger<FraudPreventionController> _logger;

        public FraudPreventionController(ILogger<FraudPreventionController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("validate")]
        public async Task<IActionResult> Validate([FromBody] PurchaseInformationRequest purchases)
        {
            //Se agrega un try/catch para atajar errores inesperados y loggearlos
            try
            {
                // Si el array esta vacio, devuelve un bad request
                if (purchases == null || purchases.Purchases == null || purchases.Purchases.Count < 2)
                    return BadRequest("Son necesarias minimo 2 ordenes para comparar.");

                // Nueva lista con la lista de ordenes frudulentas
                var fraudulentOrders = new List<PurchaseInformation>();

                // Se recorre la lista de ordenes comparandolas unas con otras
                for (int i = 0; i < purchases.Purchases.Count - 1; i++)
                {
                    for (int j = i + 1; j < purchases.Purchases.Count; j++)
                    {
                        
                        // Si son fraudulentas, se agregan a la nueva lista para devolver en el response
                        if (AreOrdersFraudulent(purchases.Purchases[i], purchases.Purchases[j]))
                        {
                            
                            fraudulentOrders.Add(purchases.Purchases[i]);
                            fraudulentOrders.Add(purchases.Purchases[j]);
                            
                            
                        }
                    }
                }
                // Verificar si una orden ya está en la lista de órdenes fraudulentas
                fraudulentOrders = fraudulentOrders.Distinct().ToList();

                // Ordena la lista de órdenes fraudulentas por OrderId en Ascendente
                fraudulentOrders = fraudulentOrders.OrderBy(order => order.OrderId).ToList();

                return Ok(fraudulentOrders);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An unexpected error occurred while processing the request.");
                return StatusCode(500, "An unexpected error occurred while processing the request.");
            }

        }

        // Funcion utilizada para comparara entre dos ordenes y devolver un verdadero si son fraudulentas
        private bool AreOrdersFraudulent(PurchaseInformation order1, PurchaseInformation order2)
        {
            
            // Comparación que no distingue entre mayúsculas y minúsculas para el correo electrónico
            string email1 = order1.EmailAddress.ToLowerInvariant().Split('@')[0].Replace(".", "").Split('+')[0];
            string email2 = order2.EmailAddress.ToLowerInvariant().Split('@')[0].Replace(".", "").Split('+')[0];

            bool emailMatch = email1.Equals(email2);

            // Comparación que no distingue entre mayúsculas y minúsculas para dirección, ciudad, estado, código postal
            bool addressMatch = order1.StreetAddress.Equals(order2.StreetAddress, StringComparison.OrdinalIgnoreCase) ||
                                (order1.StreetAddress.Replace("St.", "Street", StringComparison.OrdinalIgnoreCase) ==
                                order2.StreetAddress.Replace("St.", "Street", StringComparison.OrdinalIgnoreCase));


            bool cityMatch = order1.City.Equals(order2.City, StringComparison.OrdinalIgnoreCase);

            string state1 = order1.State.ToUpperInvariant();
            string state2 = order2.State.ToUpperInvariant();

            bool stateMatch = order1.State.Equals(order2.State, StringComparison.OrdinalIgnoreCase) ||
                                (order1.State == "NY" && state2 == "NEW YORK") ||
                                (order2.State == "NY" && state1 == "NEW YORK");

            bool zipMatch = order1.ZipCode.Equals(order2.ZipCode, StringComparison.OrdinalIgnoreCase);

            return (emailMatch && order1.DealId.Equals(order2.DealId) && (order1.CreditCardNumber != order2.CreditCardNumber)) || (addressMatch && cityMatch && stateMatch && zipMatch &&
                    order1.DealId.Equals(order2.DealId) &&
                    (order1.CreditCardNumber != order2.CreditCardNumber));
            
        }
    }
}
