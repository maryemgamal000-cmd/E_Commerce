using AutoMapper;
using E_Commerce.Application.Common;
using E_Commerce.Application.Contracts;
using E_Commerce.Application.DTOs.Products;
using E_Commerce.Application.Specifications;
using E_Commerce.Domain.Entities.Products;
using E_Commercr.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Services
{
    internal class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork  , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IReadOnlyList<BrandDto>>> GetAllBrandAsync(CancellationToken ct = default)
        {
            var brands = await _unitOfWork.GetRepository<ProductBrand, int>().GetAllAsync(ct);
            var data = _mapper.Map<IReadOnlyList<BrandDto>>(brands);
            return Result<IReadOnlyList<BrandDto>>.Ok(data);
        }

        public async Task<Result<PaginatedResult<ProductDto>>> GetAllProductsAsync(ProductQueryParams queryParams,CancellationToken ct = default)
        {
            var spec = new ProductWithTypeAndBrandSpec(queryParams);
            var products = await _unitOfWork.GetRepository<Product, int>().GetAllAsync(spec);
            var data = _mapper.Map<IReadOnlyList<ProductDto>>(products);
            var countSpec = new ProdutCountSpecifications(queryParams);
            var countOfAllProducts = await _unitOfWork.GetRepository<Product, int>().CountAsync(countSpec);
            var result = new PaginatedResult<ProductDto>(queryParams.PageIndex, queryParams.PageSize, countOfAllProducts, data);
            return Result<PaginatedResult<ProductDto>>.Ok(result);
        }

   
        public  async Task<Result<IReadOnlyList<TypeDto>>> GetAllTypesAsync(CancellationToken ct = default)
        {
            var types = await _unitOfWork.GetRepository<ProductType, int>().GetAllAsync(ct);
            var data = _mapper.Map<IReadOnlyList<TypeDto>>(types);
            return Result<IReadOnlyList<TypeDto>>.Ok(data);
        }

        public async Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken ct = default)
        {
            var spec = new ProductWithTypeAndBrandSpec(id);

            var product = await _unitOfWork.GetRepository<Product, int>().GetByIdAsync(spec , ct);

            if (product == null)
            {
                return Error.NotFound("Product.NotFound", $"Product With Id {id} Is Not Found");
            }
              
           return _mapper.Map<ProductDto>(product);
        }
    }
}
