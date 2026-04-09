const menus = [{
    name: "list",
    icon: "barcode",
    path: "/barcode/list",
    displayName: "条码管理",
    component: () =>
        import("@/components/barcode/Index.vue"),
    meta: {
        requirePermission: "Barcode.Module",
    },
}];
export default menus;