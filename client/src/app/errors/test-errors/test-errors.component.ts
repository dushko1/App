import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  baseUrl='https://localhost:5001/api/';
  validationErrors:string[]=[];

  constructor(private http:HttpClient, private toastr:ToastrService) { }

  ngOnInit(): void {
  }

  get404Error(){
    this.http.get(this.baseUrl+'buggy/not-found').subscribe(
      response=>{
        console.log(response);
      },error=>{
        
        this.toastr.error(error.statusText==="OK"?"Not found":error.statusText,error.status);
      })
  }
  get400Error(){
    this.http.get(this.baseUrl+'buggy/bad-request').subscribe(
      response=>{
        console.log(response);
      },error=>{
        this.toastr.error(error.statusText==="OK"?"Bad request":error.statusText,error.status);
      })
  }
  get500Error(){
    this.http.get(this.baseUrl+'buggy/server-error').subscribe(
      response=>{
        console.log(response);
      },error=>{
        this.toastr.error(error.statusText==="OK"?"Internal server error":error.statusText,error.status);
      })
  }
  get401Error(){
    this.http.get(this.baseUrl+'buggy/auth').subscribe(
      response=>{
        console.log(response);
      },error=>{
        
        this.toastr.error(error.statusText==="OK"?"Unauthorized":error.statusText,error.status);
      })
  }
  get400ValidationError(){
    this.http.post(this.baseUrl+'account/register',{}).subscribe(
      response=>{
        console.log(response);
      },error=>{
       console.log(error);
       this.validationErrors=error;
      })
  }

}
