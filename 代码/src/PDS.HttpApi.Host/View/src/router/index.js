import Vue from 'vue';
import VueRouter from 'vue-router';
import Home from '../views/Home.vue';
import build from './routes-builder';
import isGranted from '@/lib/permission';
import LoadConfiguration from '@/lib/configuration';
import store from '@/store';
//获取原型对象上的push函数
const originalPush = VueRouter.prototype.push
    //修改原型对象中的push方法
VueRouter.prototype.push = function push(location) {
    return originalPush.call(this, location).catch(err => err)
}
Vue.use(VueRouter);
const routes = [{
    path: '/',
    name: 'Home',
    component: Home,
    children: build(),
    redirect:"/index"
}, {
    path: "/login",
    name: "login",
    component: () =>
        import ('@/views/Login.vue')
}, {
    path: "/403",
    name: "403",
    component: () =>
        import ("@/views/403.vue")
}];
const router = new VueRouter({
    routes
});
router.beforeEach(async(to, from, next) => {
    //如果是登录,直接忽略
    if (to.path == '/login')
        next();
    let global = store.state.global;
    //刷新页面,vuex中的数据会丢失,所以要重新获取配置文件
    if (global && !global.user)
        await LoadConfiguration();
    if (to.meta.requireAuthorize) {
        if (global.user && global.user.isAuthenticated)
            next();
        else
            next("/login")
    }
    if (to.meta.requirePermission) {
        let granted = isGranted(to.meta.requirePermission);
        if (!granted)
            next("/login");
        else
            next();
    } else
        next();
});

export default router;