﻿<%@ Control Language="C#" Inherits="Yavsc.RateControl<IRating>" %>
<div data-id="<%=Model.Id%>" data-type="rate-site-skill" ><% int i = 0; for (; i<NbFilled; i++) { 
%><i class="fa fa-star" ></i><% } 
%><% if (HasHalf) {  %><i class="fa fa-star-half-o"></i><% i++; } 
%><% for (int j=0; j<NbEmpty; j++, i++ ) { %><i class="fa fa-star-o"></i><%  } 
%></div>