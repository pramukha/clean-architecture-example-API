using AutoMapper;
using Domain.Entities;
using Application.DTOs;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PlayerSkill, PlayerSkillDto>()
                .ForMember(dest => dest.Skill, opt => 
                    opt.MapFrom(src => src.Skill.ToLower()));

            CreateMap<PlayerSkillDto, PlayerSkill>()
                .ForMember(dest => dest.Skill, opt => 
                    opt.MapFrom(src => src.Skill.ToLower()));

            CreateMap<Player, PlayerDto>()
                .ForMember(dest => dest.Position, opt => 
                    opt.MapFrom(src => src.Position.ToLower()));

            CreateMap<PlayerDto, Player>()
                .ForMember(dest => dest.Position, opt => 
                    opt.MapFrom(src => src.Position.ToLower()));
        }
    }
}
