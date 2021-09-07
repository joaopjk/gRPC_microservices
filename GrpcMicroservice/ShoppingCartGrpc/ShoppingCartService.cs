using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShoppingCartGrpc.Data;
using ShoppingCartGrpc.Models;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc
{
    public class ShoppingCartService : ShoppingCartProtoService.ShoppingCartProtoServiceBase
    {
        private readonly ShoppingCartContext _shoppingCartDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(ShoppingCartContext shoppingCartDbContext, IMapper mapper, ILogger<ShoppingCartService> logger)
        {
            _shoppingCartDbContext = shoppingCartDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<ShoppingCartModel> GetShoppingCart(GetShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart =
                await _shoppingCartDbContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == request.Username);

            if (shoppingCart == null)
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Shopping Cart whit UserName={request.Username} not found"));

            var shoppingCartModel = _mapper.Map<ShoppingCartModel>(shoppingCart);
            return shoppingCartModel;
        }

        public override async Task<ShoppingCartModel> CreateShoppingCart(ShoppingCartModel request, ServerCallContext context)
        {
            var shoppingCart = _mapper.Map<ShoppingCart>(request);
            var isExist = await _shoppingCartDbContext.ShoppingCart.AnyAsync(s => s.UserName == shoppingCart.UserName);
            if (isExist)
                throw new RpcException(new Status(StatusCode.NotFound,
                    "Shopping Cart already exists"));

            _shoppingCartDbContext.ShoppingCart.Add(shoppingCart);
            await _shoppingCartDbContext.SaveChangesAsync();

            return _mapper.Map<ShoppingCartModel>(shoppingCart);
        }

        public override async Task<AddItemIntoShoppingCartResponse> AddItemIntoShoppingCart(IAsyncStreamReader<AddItemIntoShoppingCartRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var shoppingCart = await _shoppingCartDbContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == requestStream.Current.Username);
                if (shoppingCart == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={requestStream.Current.Username} is not found."));
                }

                var newAddedCartItem = _mapper.Map<ShoppingCartItem>(requestStream.Current.NewCartItem);
                var cartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == newAddedCartItem.ProductId);
                if (null != cartItem)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    //var discount = await _discountService.GetDiscount(requestStream.Current.DiscountCode);
                    //newAddedCartItem.Price -= discount.Amount;
                    float discount = 100;
                    newAddedCartItem.Price -= discount;

                    shoppingCart.Items.Add(newAddedCartItem);
                }
            }

            var insertCount = await _shoppingCartDbContext.SaveChangesAsync();

           return new AddItemIntoShoppingCartResponse
            {
                Success = insertCount > 0,
                InsertCount = insertCount
            };
        }

        public override async Task<RemoveItemIntoShoppingCartResponse> RemoveItemIntoShoppingCart(RemoveItemIntoShoppingCartRequest request, ServerCallContext context)
        {
            var shoppingCart = await _shoppingCartDbContext.ShoppingCart.FirstOrDefaultAsync(s => s.UserName == request.Username);
            if (shoppingCart == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"ShoppingCart with UserName={request.Username} is not found."));
            }

            var removeCartItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == request.RemoveCartItem.ProductId);
            if (null == removeCartItem)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"CartItem with ProductId={request.RemoveCartItem.ProductId} is not found in the ShoppingCart."));
            }

            shoppingCart.Items.Remove(removeCartItem);

            var removeCount = await _shoppingCartDbContext.SaveChangesAsync();

            var response = new RemoveItemIntoShoppingCartResponse
            {
                Success = removeCount > 0
            };

            return response;
        }
    }
}
