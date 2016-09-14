using System;

using net_product_webservice.Client.Repositories;
using net_product_webservice.Dto.ProductHierarchy;

using WebserviceClientToolkit.ClientRepositories;

namespace UserGroupsCsvToJson
{
    public class ProductTypeStore
    {
        private readonly ProductTypeRepository _repository;

        public ProductTypeStore(ProductTypeRepository repository)
        {
            _repository = repository;
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
    }
}