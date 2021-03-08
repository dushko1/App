using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extentions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDTO>().ForMember(destinationMember=>destinationMember.PhotoUrl,memberOptions=>memberOptions
            .MapFrom(sourceMember=>sourceMember.Photos.FirstOrDefault(x=>x.isMain).url))
            .ForMember(destinationMember=>destinationMember.Age,memberOptions=>memberOptions
            .MapFrom(sourceMember=>sourceMember.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDTO>();
        }
    }
}