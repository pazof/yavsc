<%@ Control Language="C#" Inherits="Yavsc.RateControl<IRating>" %>
<% Rate = Model.Rate; %>
<div data-id="<%=Model.Id%>" data-type="rate-user-skill" ><% int i = 0; for (; i<NbFilled; i++) { 
%><i class="fa fa-star" ></i><% } 
%><% if (HasHalf) {  %><i class="fa fa-star-half-o"></i><% i++; } 
%><% for (int j=0; j<NbEmpty; j++, i++ ) { %><i class="fa fa-star-o"></i><%  } 
%></div>