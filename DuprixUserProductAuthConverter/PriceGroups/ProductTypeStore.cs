using System;
using System.Collections.Generic;
using System.Linq;

using log4net;

using net_product_webservice.Client.Repositories;
using net_product_webservice.Dto.ProductHierarchy;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class ProductTypeStore
    {
        private readonly ProductTypeRepository _repository;
        private readonly ProductHierarchyRepository _productHierarchyRepository;
        private readonly ProductRepository _productRepository;
        private readonly ILog _logger;

        public ProductTypeStore(ProductTypeRepository repository, ProductHierarchyRepository productHierarchyRepository,  ProductRepository productRepository, ILog logger)
        {
            _repository = repository;
            _productHierarchyRepository = productHierarchyRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public string GetProductTypeName(int id)
        {
            RepositoryResult<ProductTypeDto> result = _repository.GetByIdAsync(id).Result;
            if(result.Success)
                return result.Result.Name;
            throw new Exception($"Failed to retrieve product type for {id}");
        }

        public RepositoryResult<ProductTypeDto> GetProductType(int id)
        {
            RepositoryResult<ProductTypeDto> result = _repository.GetByIdAsync(id).Result;
            return result;
        }

        public IEnumerable<ProductTypeProductRelationDto> GetProductTypesFor(IEnumerable<int> productIds)
        {
            var productTypeProductRelations = new List<ProductTypeProductRelationDto>();

            foreach (int productId in productIds)
            {
                var result = _productRepository.GetProductHierarchyByIdAsync(productId).Result;
                if (result.Success)
                    productTypeProductRelations.Add(
                         new ProductTypeProductRelationDto {
                             ProductType = result.Result.ProductType,
                             ProductId = result.Result.Product.Id
                         });
                else
                {
                    _logger.Error($"Failed to retrieve product hierarchy for product {productId}");
                }
            }
             
            return productTypeProductRelations;
        }
    }
}