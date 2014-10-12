<%@ Page Title="Devis" Language="C#" Inherits="System.Web.Mvc.ViewPage<Estimate>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary("Devis") %>
<% using  (Html.BeginForm("Estimate","FrontOffice")) { %>
<%= Html.LabelFor(model => model.Title) %>:<br/>
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %>
<% if (Model.Id > 0) { %>
<br/>
<%= Html.LabelFor(model => model.Owner) %>:<%=Model.Owner%>
<%= Html.ValidationMessage("Owner", "*") %>
<br/>
<%= Html.LabelFor(model => model.Ciffer) %>:<%=Model.Ciffer%>
<br/>
<%= Html.LabelFor(model => model.Id) %>:<%=Model.Id%>
<%= Html.Hidden( "Id" ) %>
<br/>

<% if (Model.Lines ==null || Model.Lines.Length == 0) { %>
<i>Pas de ligne.</i>
<%
} else { %>
<ul>
<% foreach (Writting wr in Model.Lines) { %>
<li><%=wr.Count%>/<%=wr.Id%>/<%=wr.ProductReference%>/<%=wr.UnitaryCost%>/<%=wr.Description%></li>
<%    } %>
</ul>
<%   } %>
<%  } %>
<% if (Model.Id==0) { %>
   <input type="submit" name="submit" value="Create"/>
<% } else { %>
   <input type="submit" name="submit" value="Update"/>
   <% }
   %>

<% } %>

</asp:Content>


