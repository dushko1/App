using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]


    public class UsersController : BaseApiController
    {
        private readonly DataContext context;
        private readonly IUserRepository repository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository repository, IMapper mapper, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.mapper = mapper;
            this.repository = repository;
        }
        [HttpGet]

        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user=await repository.GetUserByUsernameAsync(User.GetUsername());
            userParams.currentUsername=user.UserName;
            if(string.IsNullOrEmpty(userParams.gender)){
                userParams.gender=user.Gender=="male" ? "female" : "male";
            }
            var users = await repository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.Currentpage,users.PageSize,users.TotalCount,users.Totalpages);


            return Ok(users);

        }

        [HttpGet("{username}", Name="GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {

            return await repository.GetMemberAsync(username);

        }
        [HttpPut]
        public async Task<ActionResult> updateUser(MemberUpdateDto memberUpdateDto)
        {

            var username = User.GetUsername();
            var user = await repository.GetUserByUsernameAsync(username);
            mapper.Map(memberUpdateDto, user);
            repository.Update(user);
            if (await repository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed to update user");

        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> addPhoto(IFormFile file)
        {
            var user = await repository.GetUserByUsernameAsync(User.GetUsername());
            var result = await photoService.AddPhotoAsync(file);
            if(result.Error!=null) return BadRequest(result.Error.Message);

            var photo=new Photo{
                url=result.SecureUrl.AbsoluteUri,
                PublicId=result.PublicId
            };
            if(user.Photos.Count==0)
            {
                photo.isMain=true;
            }
            user.Photos.Add(photo);
            if(await repository.SaveAllAsync())
                {
                    // return mapper.Map<PhotoDTO>(photo);
                    return CreatedAtRoute("GetUser",new {username=user.UserName}, mapper.Map<PhotoDTO>(photo));

                }
                
            
            return BadRequest("Problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){

            var user=await repository.GetUserByUsernameAsync(User.GetUsername());

            var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo.isMain) return BadRequest("This is alrady main");

            var currentMain=user.Photos.FirstOrDefault(x=>x.isMain);
            if(currentMain!=null) currentMain.isMain=false;
            photo.isMain=true;

            if(await repository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo"); 
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> deletePhoto(int photoId)
        {
        var user=await repository.GetUserByUsernameAsync(User.GetUsername());
        var photo=user.Photos.FirstOrDefault(x=>x.Id==photoId);
        if(photo==null) return NotFound();
        if(photo.isMain) return BadRequest("You cannot delete main photo");
        if(photo.PublicId!=null)
        {
            var result=await photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error !=null) return BadRequest(result.Error.Message);
            
        }
        user.Photos.Remove(photo);
        if(await repository.SaveAllAsync()) return Ok();
        return BadRequest("Failed to delete");

        }


    }
}