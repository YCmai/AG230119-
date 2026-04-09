<template>
  <a-layout class="hb-layout">
    <a-layout-sider class="default-layout-sider">
      <menu-sider :name="$t('title.store')" :menus="menus"></menu-sider>
    </a-layout-sider>
    <a-layout-content class="hb-layout-content">
      <router-view></router-view>
    </a-layout-content>
  </a-layout>
</template>
<script>
import "@/components/style/components.less";
import locale from "@/localize/store/locale";
import menus from "@/menu/modules/store";
export default {
  name: "Storage",
  i18n: {
    messages: locale,
  },
  data() {
    return {
      menus: menus,
      // menus: [
      //   {
      //     name: "list",
      //     displayName: this.$t("menus.list"),
      //     icon: "database",
      //     route: "/store/list",
      //     isGranted: true,
      //     items: [],
      //   },
      // ],
    };
  },
};
</script>
