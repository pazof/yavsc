<%@ Page Title="Front office" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<ul>
 <li><%= Html.TranslatedActionLink("Catalog") %></li>
 <li><%= Html.TranslatedActionLink("Basket" ) %> </li>
 <li><%= Html.TranslatedActionLink("YourEstimates") %></li>
 <li><%= Html.TranslatedActionLink("DoAnEstimate") %></li>
 <% if (User.Identity.IsAuthenticated) { %>
 <li><%= Html.TranslatedActionLink("Activities" ) %></li>
 <li><%= Html.TranslatedActionLink("SiteSkills") %></li>
 <li><%= Html.TranslatedActionLink("UserSkills") %></li>
 <% } else { %>
 <li> <i><%= Html.Translate("AuthenticatedOnly") %>:
 <%= Html.Translate("Activities") %>,
 <%= Html.Translate("SiteSkills") %>,
 <%= Html.Translate("UserSkills") %></i>
</li> <% }%>
 </ul>

</asp:Content>
