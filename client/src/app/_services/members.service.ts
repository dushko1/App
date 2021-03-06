import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl=environment.apiUrl;
  members:Member[]=[];
  memberCache=new Map();
  user:User;
  userParams:UserParams;
  

  constructor(private http:HttpClient,private accountService:AccountService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user=user;
      this.userParams=new UserParams(user);
    })
  }

  getUserParams(){
    return this.userParams;
  }

  setUserParams(params:UserParams)
  {
    this.userParams=params;
  }

  resetUserParams(){
    this.userParams=new UserParams(this.user);
    return this.userParams
  }


  getMembers(userParms:UserParams){
    var response=this.memberCache.get(Object.values(userParms).join('-'));
    if(response){
      return of(response);
    }

    let params=this.getPaginationHeader(userParms.pageNumber,userParms.pageSize);
    params=params.append('minAge',userParms.minAge.toString());
    params=params.append('maxAge',userParms.maxAge.toString());
    params=params.append('gender',userParms.gender);
    params=params.append('orderBy',userParms.orderBy);

    return this.getPaginatedResult<Member[]>(this.baseUrl+'users',params)
    .pipe(map(response=>{
      this.memberCache.set(Object.values(userParms).join('-'),response);
      return response;
    }))
  }



  getMember(username:string){
    const member=[...this.memberCache.values()]
    .reduce((arr,elem)=>arr.concat(elem.result),[])
    .find((member:Member)=>member.username===username);
    if(member){
      return of(member);
    }
    
    return this.http.get<Member>(this.baseUrl+'users/'+username);
  }

  updateMember(member:Member){

    return this.http.put(this.baseUrl+'users',member).pipe(
      map(()=>{
        const index=this.members.indexOf(member);
        this.members[index]=member;
      })
    )

  }

  setMainPhoto(photoId:number)
  {
    return this.http.put(this.baseUrl+'users/set-main-photo/'+photoId,{})
  }
  deletePhoto(photoId:number)
  {
    return this.http.delete(this.baseUrl+'users/delete-photo/'+photoId);
  }

  private getPaginationHeader(pageNumber:number,pageSize:number){
    let params=new HttpParams();
      params=params.append('pageNumber',pageNumber.toString());
      params=params.append('pageSize',pageSize.toString());
      return params;

  }
    private getPaginatedResult<T>(url,params) {
    const paginatedResult:PaginatedResult<T>=new PaginatedResult<T>();
    return this.http.get<T>(url, {observe: 'response', params}).pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
  }
}
