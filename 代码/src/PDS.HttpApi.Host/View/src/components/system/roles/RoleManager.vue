<template>
  <!-- <base-manager @show="show" :showBtn="isGranted('AbpIdentity.Roles.Create')"> -->
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-button
        icon="plus"
        type="primary"
        size="small"
        v-if="isGranted('AbpIdentity.Roles.Create')"
        @click="show"
      >
        {{ $t("actions.add") }}
      </a-button>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.name"
        :row-selection="{ onSelect: select, type: 'radio' }"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      >
        <template slot="icon" slot-scope="text, record">
          <a-icon v-if="record.icon" :type="record.icon" />
          <a-icon v-else type="menu-fold" />
        </template>
        <template slot="displayName" slot-scope="text">
          {{ L(text) }}
        </template>
        <template slot="menuType" slot-scope="text">
          {{ L(text) }}
        </template>
        <template slot="operation" slot-scope="text, record">
          <a
            href="javascript:void(0);"
            @click="show('edit', record)"
            v-if="isGranted('AbpIdentity.Roles.Update')"
          >
            <a-icon type="edit" />{{ $t("actions.edit") }}
          </a>
          <a-popconfirm
            :title="$t('confirm.delete')"
            @confirm="remove(record)"
            v-if="isGranted('AbpIdentity.Roles.Delete') && !record.isStatic"
          >
            <a href="javascript:;">
              <a-icon type="delete" />{{ $t("actions.delete") }}
            </a>
          </a-popconfirm>
        </template>
      </a-table>
      <Form ref="form" @close="close" v-if="visible" />
    </a-layout-content>
  </a-layout>
</template>
<script>
import { Pagination, Delete } from "@/api/role";
import Form from "./RoleForm.vue";
import component from "@/lib/base";
import locale from "@/localize/system/role";
export default {
  name: "RoleList",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  components: { Form },
  data() {
    return {
      visible: false,
      columns: [
        {
          dataIndex: "name",
          title: this.$t("title.name"),
          align: "center",
        },
        {
          dataIndex: "description",
          title: this.$t("title.description"),
          align: "center",
          ellipsis: true,
        },
        {
          dataIndex: "creator",
          title: this.$t("title.creator"),
          align: "center",
          ellipsis: true,
        },
        {
          dataIndex: "createTime",
          title: this.$t("title.createTime"),
          align: "center",
          ellipsis: true,
        },
        {
          dataIndex: "remark",
          title: this.$t("title.remark"),
          align: "center",
        },
        {
          dataIndex: "Actions",
          title: this.$t("title.actions"),
          align: "center",
          scopedSlots: { customRender: "operation" },
        },
      ],
      data: [],
      loading: false,
      pagination: {
        current: 1,
        total: 0,
        pageSize: 10,
        "show-total": (total) => `共${total}条数据`,
      },
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      let _this = this;
      this.loading = true;
      try {
        let result = await Pagination({
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        _this.data = result.items;
        _this.pagination.total = result.totalCount;
      } catch (err) {
        _this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    show: function (action, record) {
      let _this = this;
      this.visible = true;
      if (record)
        this.$nextTick(() => {
          _this.$refs.form.load(record.id);
        });
    },
    close: function () {
      this.visible = false;
      this.load();
    },
    remove: async function (record) {
      let _this = this;
      let spin = this.$spin({ text: _this.$t(""), target: "#container" });
      try {
        await Delete(record.id);
        _this.$message.success(_this.$t("save.delete"), () => {});
        _this.load();
      } catch (err) {
        _this.$message.error(err.message);
      } finally {
        spin.close();
      }
    },
    select: function (record) {
      this.$emit("auth", record.name);
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
  },
};
</script>
