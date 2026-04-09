<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider name="入库管理" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import "@/components/style/components.less";
import menus from "@/menu/modules/input";
export default {
  name: "Input",
  data() {
    return {
      menus: menus,
      // menus: [
      //   {
      //     name: "line",
      //     displayName: "线边入库",
      //     icon: "code",
      //     route: "/input/line",
      //     isGranted: true,
      //     items: [],
      //   },
      // ],
    };
  },
};
</script>
