<%@ Page Title="Credits" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">



<div>

<%=Html.Translate("Icons_made_by")%> <a href="http://www.flaticon.com/authors/vectorgraphit" title="Vectorgraphit">Vectorgraphit</a> 
<%=Html.Translate("from")%>  <a href="http://www.flaticon.com" title="Flaticon">www.flaticon.com</a>             
<%=Html.Translate("is_licensed_by")%> <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0">CC BY 3.0</a>

</div>

<% foreach ( Yavsc.Helpers.Link link in Html.Thanks()) { %>
     <a href="<%=link.Url%>"><% if (link.Image !=null) { 
     %><img src="<%= link.Image %>" alt="<%= link.Text %>"/></a>
     <%  } else { %>
     <a href="<%=link.Url%>"><%= link.Text %></a>
     <% }} %>

</asp:Content>
