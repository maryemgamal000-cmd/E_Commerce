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
    internal class ProductWithTypeAndBrandSpec : BaseSpacification<Product, int>
    {
        //Get All
        public ProductWithTypeAndBrandSpec(ProductQueryParams queryParams)
           : base(P => (!queryParams.BrandId.HasValue || P.BrandId == queryParams.BrandId.Value)
             && (!queryParams.TypeId.HasValue || P.TypeId == queryParams.TypeId.Value)
             && (string.IsNullOrWhiteSpace(queryParams.SearchValue) || P.Name.ToLower().Contains(queryParams.SearchValue.ToLower())))
        {
            AddInclude(p => p.ProductType);
            AddInclude(p => p.ProductBrand);

            switch (queryParams.Sort)
            {
                case ProductSortingOptions.NameAsc:
                    AddOrderBy(P => P.Name);
                    break;

                case ProductSortingOptions.NameDesc:
                    AddOrderByDesc(P => P.Name);
                    break;

                case ProductSortingOptions.PriceAsc:
                    AddOrderBy(P => P.Price);
                    break;

                case ProductSortingOptions.PriceDesc:
                    AddOrderByDesc(P => P.Price);
                    break;

                default:
                    AddOrderBy(P => P.Id);
                    break;
            }

            ApplyPagination(queryParams.PageSize , queryParams.PageIndex);
        }

        // Get By Id
        public ProductWithTypeAndBrandSpec(int id) : base(x => x.Id == id)
        {
            AddInclude(p => p.ProductType);
            AddInclude(p => p.ProductBrand);
        }
    }
}
