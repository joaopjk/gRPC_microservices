using System.Linq;
using System.Threading.Tasks;
using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;

namespace DiscountGrpc.Service
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(d => d.Code == request.DiscountCode);

            return Task.FromResult(new DiscountModel
            {
                DiscountId = discount.DiscountId,
                Code = discount.Code,
                Amount = discount.Amount
            });
        }
    }
}
