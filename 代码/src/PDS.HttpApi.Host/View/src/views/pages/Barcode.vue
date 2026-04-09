<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider name="条码管理" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import "@/components/style/components.less";
import component from "@/lib/base";
import menus from "@/menu/modules/barcode";
export default {
  name: "Barcode",
  mixins: [component],
  data() {
    return {
      menus: menus,
      //  [
      //   {
      //     name: "list",
      //     displayName: "条码管理",
      //     icon: "barcode",
      //     route: "/barcode/list",
      //     isGranted: true,
      //     items: [],
      //   },
      // ],
    };
  },
};
</script>
