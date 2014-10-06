<%@ Page Title="Catalog" Language="C#"  Inherits="System.Web.Mvc.ViewPage<PhysicalProduct>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = Model.Name; %>
</asp:Content>

<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
<i><%= Html.Encode(Model.Reference) %></i></asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div class="product">
<p><%= Html.Encode(Model.Description) %></p>
<% if (Model.Images!=null) foreach (ProductImage i in Model.Images) { %> 
<img src="<%=i.Src%>" alt="<%=i.Alt%>"/>
<% } %>
<% if (Model.UnitaryPrice !=null) { %>
Prix unitaire : <%= Html.Encode(Model.UnitaryPrice.Quantity.ToString())%> 
<%= Html.Encode(Model.UnitaryPrice.Unit.Name)%>
<% } else { %> Gratuit! <% } %>
</div>
<div class="booking">
<% if (Model.CommandForm!=null) { %>
Défaut de formulaire de commande!!!
<% } else { Response.Write( Html.CommandForm(Model,"Ajouter au panier")); } %>

<% if (Model.CommandValidityDates!=null) { %>
Offre valable du <%= Model.CommandValidityDates.StartDate.ToString("dd/MM/yyyy") %> au 
<%= Model.CommandValidityDates.EndDate.ToString("dd/MM/yyyy") %>.
<% } %>

</div>
</asp:Content>
