const menus = [{
    icon: "ordered-list",
    name: "list",
    path: "/warehouse/list",
    displayName: "库别列表",
    component: () =>
        import("@/components/warehouse/Index.vue"),
    meta: {
        requirePermission: "Barcode.Module",
    },
}];
export default menus;