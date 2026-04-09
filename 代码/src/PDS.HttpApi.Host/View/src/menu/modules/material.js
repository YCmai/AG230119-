const menus = [{
    icon: "bars",
    name: "stock",
    displayName: "库存管理",
    meta: {
        requirePermission: "Material.Module.Stock",
    },
    children: [{
        icon: "profile",
        name: "in",
        path: "/material/stock/in",
        displayName: "库存明细",
        routePath: "stock/in",
        meta: {
            requirePermission: "Material.Module.Stock.Detail",
        },
        component: () =>
            import("@/components/stock/StoreInWarehouse.vue")
    }, {
        icon: "edit",
        name: "out",
        path: "/material/stock/out",
        displayName: "出库明细",
        routePath: "stock/out",
        requirePermission: "",
        meta: {
            requirePermission: "Material.Module.Stock.Output",
        },
        component: () =>
            import("@/components/stock/OutOfWarehouse.vue")
    }, {
        icon: "database",
        name: "statistic",
        path: "/material/stock/statistic",
        displayName: "库存统计",
        routePath: "stock/statistic",
        meta: {
            requirePermission: "Material.Module.Stock.Statis",
        },
        component: () =>
            import("@/components/stock/Statistic.vue")
    }]
}, {
    icon: "scan",
    name: "list",
    displayName: "物料维护",
    path: "/material/list",
    component: () =>
        import("@/components/material/Index.vue"),
    meta: {
        requirePermission: "Material.Module.Management",
    }
}, {
    icon: "plus",
    name: "increase",
    displayName: "新增库存",
    path: "/material/Increase",
    component: () =>
        import("@/components/stock/IncreaseInventory.vue"),
    meta: {
        requirePermission: "Material.Module.AddStock",
    }
}, {
    icon: "edit",
    name: "edit",
    displayName: "修改可用数量",
    path: "/material/edit",
    component: () =>
        import("@/components/stock/EditAvailableQuatity.vue"),
    meta: {
        requirePermission: "Material.Module.ModifyUsableQuantity",
    }
}, {
    icon: "read",
    name: "modifyRecord",
    displayName: "修改物料记录",
    path: "/material/modifyRecord",
    component: () =>
        import("@/components/stock/MaterialModifyRecord.vue"),
    meta: {
        requirePermission: "Material.Module.ModifyRcord",
    }
}];
export default menus;