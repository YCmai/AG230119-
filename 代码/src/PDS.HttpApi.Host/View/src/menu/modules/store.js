const menus = [{
    icon: "database",
    name: "list",
    path: "/store/list",
    requirePermission: "",
    displayName: "储位列表",
    meta: {
        requirePermission: "Storage.Module",
    },
    component: () =>
        import ("@/components/store/Index.vue")
}];
export default menus;