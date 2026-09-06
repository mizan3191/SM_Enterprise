//namespace DEALER.DataAccess
//{
//    public class PreOrderManager : BaseDataManager, IPreOrder
//    {
//        public PreOrderManager(DEALERContext model) : base(model)
//        {
//        }

//        public bool CreatePreOrder(PreOrder PreOrder)
//        {
//            return AddUpdateEntity<PreOrder>(PreOrder);
//        }

//        public bool UpdatePreOrder(PreOrder PreOrder)
//        {
//            return AddUpdateEntity<PreOrder>(PreOrder);
//        }


//        public bool DeletePreOrder(int id)
//        {
//            return RemoveEntity<PreOrder>(id);
//        }

//        public async Task<PreOrder> GetPreOrderById(int PreOrderId)
//        {
//            try
//            {
//                return await _dbContext.PreOrders
//                .Include(o => o.PreOrderDetails)
//                .FirstOrDefaultAsync(o => o.Id == PreOrderId);
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public async Task<IEnumerable<PreOrdersDTO>> GetAllPreOrders()
//        {
//            try
//            {
//                var PreOrders = await _dbContext.PreOrders
//                .Include(o => o.PreOrderDetails)
//                .Include(o => o.Customer)
//                .Select(o => new PreOrdersDTO
//                {
//                    Id = o.Id,
//                    ProductName = string.Join(", ", o.PreOrderDetails.Select(od => od.Product.Name)),
//                    CustomerName = o.Customer.Name,
//                    OrderDate = o.Date,
//                    TotalPrice = o.TotalAmount,
//                })
//                .OrderByDescending(o => o.Id)
//                .ToListAsync();

//                return PreOrders;
//            }
//            catch (Exception ex)
//            {
//                return Enumerable.Empty<PreOrdersDTO>();
//            }
//        }
//    }
//}