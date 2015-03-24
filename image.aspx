<%@ Page Language="C#" AutoEventWireup="true" CodeFile="image.aspx.cs" Inherits="image_aspx" MasterPageFile="MP_MasterPage/MasterPage_NoUI .master" %>

<asp:Content runat="server" ContentPlaceHolderID="head">
    <script src="<%=ResolveUrl("~/js/image.js") %>"></script>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="body">
    <%
        if (PrevID != 0)
        { 
    %>
    <a href="/image/<%=PrevID %>">上一张</a>
    <%
        }
        if (NextID != 0)
        {
    %>
    <a href="/image/<%=NextID %>">下一张</a>
    <%
        }
    %>
</asp:Content>
