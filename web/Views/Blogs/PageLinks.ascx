<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<%=  (int) ViewData["ResultCount"] %>
   rp1.PageSize = (int) ViewData ["PageSize"]; 
   rp1.PageIndex = (int) ViewData["PageIndex"]; 
   rp1.None = Html.Translate("no content"); 

	 	 	 