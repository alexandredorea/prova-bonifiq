using Microsoft.AspNetCore.Mvc;
using ProvaPub.Models;
using ProvaPub.Services;

namespace ProvaPub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class Parte2Controller : ControllerBase
    {
        /*
         * Controller com muitas responsabilidades, injetando muitos services para contextos distindo.
         * Sugestão seria quebrar em duas controllers especificas.
         *
         * Ideia:
         * https://next.sonarqube.com/sonarqube/coding_rules?open=csharpsquid%3AS6960&rule_key=csharpsquid%3AS6960
         */
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;

        /// <summary>
        /// Precisamos fazer algumas alterações:
        /// 1 - Não importa qual page é informada, sempre são retornados os mesmos resultados. Faça a correção.
        /// 2 - Altere os códigos abaixo para evitar o uso de "new", como em "new ProductService()". Utilize a Injeção de Dependência para resolver esse problema
        /// 3 - Dê uma olhada nos arquivos /Models/CustomerList e /Models/ProductList. Veja que há uma estrutura que se repete.
        /// Como você faria pra criar uma estrutura melhor, com menos repetição de código? E quanto ao CustomerService/ProductService. Você acha que seria possível evitar a repetição de código?
        ///
        /// </summary>
		public Parte2Controller(
            IProductService productService,
            ICustomerService customerService)
        {
            _productService = productService;
            _customerService = customerService;
        }

        [HttpGet("products")]
        public async Task<PagedResult<Product>> ListProducts(int page, CancellationToken cancellationToken)
        {
            if (page <= 0) page = 1;
            return await _productService.ListProducts(page, cancellationToken);
        }

        [HttpGet("customers")]
        public async Task<PagedResult<Customer>> ListCustomers(int page, CancellationToken cancellationToken)
        {
            if (page <= 0) page = 1;
            return await _customerService.ListCustomers(page, cancellationToken);
        }
    }
}