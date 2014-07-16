<%@ Page Title="Catalog" Language="C#"  Inherits="System.Web.Mvc.ViewPage<Service>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
<title><%= Html.Encode(Model.Name) %></title>
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
<h1><%=ViewData ["BrandName"]%> - <%=ViewData ["ProdCatName"]%> - <%= Html.ActionLink( Model.Name, "Product", new { id = ViewData ["BrandName"],  pc = ViewData ["ProdCatRef"] , pref = Model.Reference } ) %></h1>

</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="service">
<p><%= Html.Encode(Model.Description) %></p>
<% if (Model.Images!=null) foreach (ProductImage i in Model.Images) { %> 
<img src="<%=i.Src%>" alt="<%=i.Alt%>"/>
<% } %>
<% if (Model.HourPrice !=null) { %>
Prix horaire de la prestation :
<%= Html.Encode(Model.HourPrice.Quantity.ToString())%>
<%= Html.Encode(Model.HourPrice.Unit.Name)%>
<% } %>
</div>

<div class="booking">
<%= Html.CommandForm(Model,"Ajouter au panier") %>

<% if (Model.CommandValidityDates!=null) { %>
Offre valable du <%= Model.CommandValidityDates.StartDate.ToString("dd/MM/yyyy") %> au 
<%= Model.CommandValidityDates.EndDate.ToString("dd/MM/yyyy") %>.
<% } %>

</div>
</asp:Content>
