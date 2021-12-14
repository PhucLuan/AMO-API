using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.DataAccessor.Entities;

namespace Rookie.AMO.Business
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            FromDataAccessorLayer();
            FromPresentationLayer();
            CreateMap<AssetDto, Asset>()
                .ForMember(x => x.Category, ig => ig.Ignore())
                .ForMember(x => x.Assignments, ig => ig.Ignore())
                .ForMember(x => x.CreatedDate, ig => ig.Ignore())
                .ForMember(x => x.UpdatedDate, ig => ig.Ignore())
                .ForMember(x => x.CreatorId, ig => ig.Ignore())
                .ForMember(x => x.Pubished, ig => ig.Ignore());
            CreateMap<Asset, AssetDto>();
            CreateMap<AssignmentDto, Assignment>()
                .ForMember(x => x.Asset, ig => ig.Ignore())
                .ForMember(x => x.RequestAssignment, ig => ig.Ignore())
                .ForMember(x => x.CreatedDate, ig => ig.Ignore())
                .ForMember(x => x.UpdatedDate, ig => ig.Ignore())
                .ForMember(x => x.Pubished, ig => ig.Ignore());
            //CreateMap<Assignment, AssignmentDto>();    
        }

        private void FromPresentationLayer()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(x => x.Desc, ig => ig.Ignore());
            CreateMap<AssignmentRequest, Assignment>()
                .ForMember(x => x.CreatedDate, ig => ig.Ignore())
                .ForMember(x => x.UpdatedDate, ig => ig.Ignore())
                .ForMember(x => x.Pubished, ig => ig.Ignore())
                .ForMember(x => x.Id, ig => ig.Ignore())
                .ForMember(x => x.State, ig => ig.Ignore())
                .ForMember(x => x.Asset, ig => ig.Ignore())
                .ForMember(x => x.RequestAssignment, ig => ig.Ignore())
                .ForMember(x => x.RequestAssignmentId, ig => ig.Ignore())
                .ForMember(x => x.CreatorId, ig => ig.Ignore());

            CreateMap<RequestReturn, RequestAssignment>()
                .ForMember(x => x.ReturnDate, ig => ig.Ignore())
                .ForMember(x => x.State, ig => ig.Ignore())
                .ForMember(x => x.UserRequestId, ig => ig.Ignore())
                .ForMember(x => x.UserAcceptId, ig => ig.Ignore())
                .ForMember(x => x.Assignment, ig => ig.Ignore());
        }

        private void FromDataAccessorLayer()
        {
            //CreateMap<Category, CategoryDto>();
            CreateMap<Assignment, AssignmentDto>()
                .ForMember(dest => dest.AssetCode, opt => opt
                    .MapFrom(s => s.Asset.Code))
                .ForMember(dest => dest.AssetName, opt => opt
                    .MapFrom(s => s.Asset.Name))
                .ForMember(dest => dest.AssetSpecification, opt => opt
                    .MapFrom(s => s.Asset.Specification))
                .ForMember(p => p.AssignedTo,  x => x.Ignore())
                .ForMember(p => p.AssignedBy, x=> x.Ignore())
                .ForMember(p => p.CategoryName, x => x.Ignore())
                .ForMember(p => p.StateName, x => x.Ignore());

            CreateMap<RequestAssignment, RequestDto>()
                .ForMember(x => x.AssignmentDto, ig => ig.Ignore());
            CreateMap<RequestDto, RequestAssignment>()
                .ForMember(x => x.Assignment, ig => ig.Ignore());
        }
    }
}
