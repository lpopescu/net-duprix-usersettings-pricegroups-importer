using System;
using System.Collections.Generic;
using System.Linq;

using net_product_webservice.Client.Repositories;
using net_product_webservice.Dto.ProductHierarchy;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class ProductTypeStore
    {
        private readonly ProductTypeRepository _repository;
        private readonly ProductRepository _productRepository;

        public ProductTypeStore(ProductTypeRepository repository, ProductRepository productRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
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

        public IEnumerable<ProductTypeProductRelationDto> GetProductTypeId(IEnumerable<int> productIds)
        {
            var result = _productRepository.GetProductHierarchyByIdsAsync(productIds).Result;
            if (result.Success)
                return result.Result.Select( ph => new ProductTypeProductRelationDto { ProductId = ph.Product.Id, ProductType = ph.ProductType});

            throw new Exception($"Failed to retrieve product hierarchy for {string.Join(",",productIds)}");
        }
    }
}