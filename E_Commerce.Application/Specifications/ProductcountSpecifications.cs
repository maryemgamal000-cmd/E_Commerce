using E_Commerce.Application.Common;
using E_Commerce.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Specifications
{
    internal class ProdutCountSpecifications : BaseSpacification<Product, int>
    {
        public ProdutCountSpecifications(ProductQueryParams queryParams)
             : base(P => (!queryParams.BrandId.HasValue || P.BrandId == queryParams.BrandId.Value)
             && (!queryParams.TypeId.HasValue || P.TypeId == queryParams.TypeId.Value)
             && (string.IsNullOrWhiteSpace(queryParams.SearchValue) || P.Name.ToLower().Contains(queryParams.SearchValue.ToLower())))

        {
        }
    }
}
