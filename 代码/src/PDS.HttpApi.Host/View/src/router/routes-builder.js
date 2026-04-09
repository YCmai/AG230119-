import menus from '@/menu/main';

export default function() {
    let routes = build(menus);
    return routes;
}

function build(menus) {
    let routes = [];
    menus.forEach(menu => {
        const route = {
            name: menu.name,
            path: menu.routePath ? menu.routePath : menu.name,
            component: menu.component,
            //todo 这样直接跳转无法判断权限,应该在路由进入前判断!
            redirect: menu.redirect,
            meta: menu.meta || {},
        };
        //todo 这里有个问题?如果菜单是包含子菜单,router-view会不显示,是因为vue-router的上下级引起的问题
        //是否能通过改变路由地址去修改？待尝试!
        if (menu.children && menu.children.length > 0) {
            if (menu.path)
                route.children = build(menu.children);
            else {
                menu.children.forEach(child => {
                    routes.push({
                        name: child.name,
                        path: child.routePath ? child.routePath : child.name,
                        component: child.component,
                        meta: child.meta || {},
                    })
                });
            }
        }
        routes.push(route);
    });
    return routes;
}