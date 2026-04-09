const menus = [{
    icon: "code",
    name: "line",
    displayName: "线边入库",
    path: "/input/line",
    component: () =>
        import ("@/components/line-input/Index.vue"),
    meta: {
        requirePermission: "LineCall.Module.LineCallInput"
    }
}];
export default menus;