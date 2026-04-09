import system from './modules/system';
import barcode from './modules/barcode';
import material from './modules/material';
import store from './modules/store';
import warehouse from './modules/ware-house';
import input from './modules/input';
//import $t from './menu-display';
//中英文转换
const menus = [{
    icon:"home",
    name:"index",
    path:"/index",
    displayName:"主页",
    component:()=>
        import("@/views/pages/Index.vue")
},{
    icon: "form",
    name: "input",
    path: "/input",
    displayName: "入库管理",
    component: () =>
        import("@/views/pages/Input.vue"),
    children: input,
    redirect: "/input/line" ,
    meta: {
        requirePermission: "LineCall.Module"
    }
}, {
    icon: "bank",
    name: "warehouse",
    path: "/warehouse",
    displayName: "库别管理",
    component: () =>
        import("@/views/pages/WareHouse.vue"),
    children: warehouse,
    redirect: "/warehouse/list",
    meta: {
        requirePermission: "WareHouse.Module"
    }
}, {
    icon: "appstore",
    name: "store",
    path: "/store",
    displayName: "储位管理",
    component: () =>
        import("@/views/pages/Store.vue"),
    children: store,
    redirect: "/store/list",
    meta: {
        requirePermission: "Storage.Module",
    }
}, {
    icon: "scan",
    name: "material",
    displayName: "物料管理",
    path: "/material",
    component: () =>
        import("@/views/pages/Material.vue"),
    children: material,
    redirect: "/material/list",
    meta: {
        requirePermission: "Material.Module",
    }
}, {
    icon: "barcode",
    name: "barcode",
    displayName: "条码管理",
    path: "/barcode",
    component: () =>
        import("@/views/pages/Barcode.vue"),
    children: barcode,
    meta: {
        requirePermission: "Barcode.Module",
    },
    redirect: "/barcode/list"
}, {
    icon: "setting",
    name: "system",
    displayName: "系统设置",
    path: "/system",
    children: system,
    component: () =>
        import("@/views/pages/System.vue"),
    redirect: "/system/users/manager",
    meta: {
        requirePermission: "AbpIdentity.Users",
    },
}];
export default menus;