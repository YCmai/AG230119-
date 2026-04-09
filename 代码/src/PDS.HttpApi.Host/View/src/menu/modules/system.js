const system = [{
        name: "users",
        displayName: "用户管理",
        icon: "team",
        requireAuthorize: true,
        children: [{
                name: "manager",
                displayName: "用户列表",
                icon: "user",

                path: "/system/users/manager",
                routePath: "users/manager",
                component: () =>
                    import ("@/components/system/users/UserManager.vue"),
                meta: {
                    requirePermission: "AbpIdentity.Users",
                }
            },
            {
                name: "roles",
                displayName: "角色管理",
                icon: "idcard",

                path: "/system/users/roles",
                routePath: "users/roles",
                component: () =>
                    import ("@/components/system/roles/RoleIndex.vue"),
                meta: {
                    requirePermission: "AbpIdentity.Roles",
                }
            },
        ],
    },
    {
        name: "audit",
        displayName: "接口日志",
        icon: "api",
        requirePermission: "Aduit.List",
        path: "/system/audit",
        component: () =>
            import ("@/components/system/audit/Index.vue")
            //componentPath: ""
    },
    {
        name: "global",
        displayName: "全局参数",
        icon: "edit",
        requirePermission: "Setting.Global",
        path: "/system/global",
        component: () =>
            import ("@/components/system/settings/Index.vue")
    },
];
export default system;