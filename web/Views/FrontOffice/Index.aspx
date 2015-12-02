<%@ Page Title="Front office" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<ul>
 <li><%= Html.ActionLink("Catalog","Catalog" ) %></li>
 <li><%= Html.ActionLink("Basket","Basket" ) %> </li>
 <li><%= Html.ActionLink("Estimates","Estimates" ) %></li>
 <li><%= Html.ActionLink("Estimate","Estimate" ) %></li>
 <% if (User.Identity.IsAuthenticated) { %>
 <li><%= Html.ActionLink("Activities","Activities" ) %></li>
 <li><%= Html.ActionLink("ManagedSiteSkills", "Skills" ) %></li>
 <li><%= Html.ActionLink("UserSkills","UserSkills" ) %></li>
 <% } else { %>
 <li><%= Html.Translate("Activities") %>,
 <%= Html.Translate("ManagedSiteSkills") %>,
 <%= Html.Translate("UserSkills") %>:
 <i><%= Html.Translate("AuthenticatedOnly") %></i></li>
 <% }%>

 </ul>
</asp:Content>
